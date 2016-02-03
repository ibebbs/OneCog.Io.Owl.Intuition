using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using OneCog.Core.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using OneCog.Core;

namespace OneCog.Io.Owl.Intuition.Command.Endpoint
{
    public interface IInstance : IDisposable
    {
        Task<Response.Version> Send(Request.GetVersion message);
        Task<Response.Rosta> Send(Request.GetRosta message);
        Task<Response.Device> Send(Request.GetDevice message);
        Task<Response.Udp> Send(Request.GetUpdPushPort message);
        Task<Response.Udp> Send(Request.SetUdpPushPort message);
        Task<Response.Save> Send(Request.Save message);

        void Open();
        void Close();
    }

    internal class Instance : IInstance
    {
        private readonly IPEndPoint _localEndpoint;
        private readonly IPEndPoint _remoteEndpoint;
        private readonly TimeSpan _requestTimeout;
        private readonly string _udpKey;
        private readonly IScheduler _scheduler;

        private readonly Subject<ScheduledRequest> _sendQueue;
        private readonly IObservable<ScheduledRequest> _send;
        private readonly IConnectableObservable<IResponse> _responses;
        
        private IDisposable _connection;

        public Instance(Socket.IFactory socketFactory, Response.IParser responseParser, IPEndPoint localEndpoint, IPEndPoint remoteEndpoint, TimeSpan requestTimeout, string udpKey, IScheduler scheduler = null)
        {
            _localEndpoint = localEndpoint;
            _remoteEndpoint = remoteEndpoint;
            _requestTimeout = requestTimeout;
            _udpKey = udpKey;
            _scheduler = scheduler ?? new EventLoopScheduler();

            _sendQueue = new Subject<ScheduledRequest>();

            _responses = Observable
                .Using(
                    () => socketFactory.ConstructCommandReceiveSocket(localEndpoint),
                    udpReceive => Observable
                        .Return(1)
                        .SelectMany(_ => udpReceive.Receive())
                        .Select(result => result.Buffer)
                        .Select(bytes => Encoding.ASCII.GetString(bytes))
                        .Do(Instrumentation.Command.Endpoint.Receive)
                        .SelectMany(value => responseParser.GetResponses(value))
                        .Do(Instrumentation.Command.Endpoint.Response))
                        .Repeat()
                .Publish();

            _send = Observable
                .Using(
                    () => socketFactory.ConstructCommandSendSocket(localEndpoint),
                    udpSend => _sendQueue
                        .ObserveOn(_scheduler)
                        .Select(scheduledRequest => Send(udpSend, scheduledRequest))
                        .Concat());
        }

        private IObservable<ScheduledRequest> Send(Socket.IObservableUdpClient socket, ScheduledRequest scheduledRequest)
        {
            return Observable.Create<ScheduledRequest>(
                observer =>
                {
                    Instrumentation.Command.Endpoint.Request(scheduledRequest.Request);

                    string requestString = AddKey(scheduledRequest.Request.AsString());

                    Instrumentation.Command.Endpoint.Send(requestString);

                    byte[] datagram = Encoding.ASCII.GetBytes(requestString);

                    return Observable.Zip(
                        scheduledRequest.ParseResponse(_responses),
                        socket.Send(datagram, datagram.Length, _remoteEndpoint),
                        (response, sent) => scheduledRequest)
                        .Subscribe(observer);
                }
            );
        }

        public void Dispose()
        {
            Close();
        }

        private bool RequestFailed<T>(T response) where T : IResponse
        {
            return response.Status != Status.Ok;
        }

        private Exception RequestException(IRequest request, IResponse response)
        {
            Exception exception = new InvalidOperationException(string.Format("Received error in response to the following request: '{0}'", request.AsString()));

            Instrumentation.Command.Endpoint.Error(exception);

            return exception;
        }

        private string AddKey(string command)
        {
            return string.Format("{0},{1}", command, _udpKey);
        }
        
        private Task<T> ScheduledSend<T>(IRequest request) where T : class, IResponse
        {
            TaskCompletionSource<T> result = new TaskCompletionSource<T>();

            Func<IObservable<IResponse>, IObservable<IResponse>> handler =
                responses =>
                {
                    Instrumentation.Command.Endpoint.AddingReceiveHandlerFor(typeof(T));

                    return responses
                        .Do(response => Instrumentation.Command.Endpoint.ReceiveHandlerReceived(response.GetType()))
                        .OfType<T>()
                        .Do(response => Instrumentation.Command.Endpoint.ReceiveHandlerProcessing(response.GetType()))
                        .ThrowWhen(RequestFailed, response => RequestException(request, response))
                        .Timeout(_requestTimeout, _scheduler)
                        .Take(1)
                        .Do(
                            value =>
                            {
                                Instrumentation.Command.Endpoint.CompletingReceiveHandlerFor(typeof(T));
                                result.SetResult(value);
                            },
                            exception =>
                            {
                                Instrumentation.Command.Endpoint.ErroringReceiveHandlerFor(typeof(T), exception);
                                result.SetException(exception);
                            })
                        .Cast<IResponse>();
                };

            _sendQueue.OnNext(new ScheduledRequest(request, handler));

            return result.Task;
        }

        public Task<Response.Version> Send(Request.GetVersion message)
        {
            return ScheduledSend<Response.Version>(message);
        }

        public Task<Response.Rosta> Send(Request.GetRosta message)
        {
            return ScheduledSend<Response.Rosta>(message);
        }

        public Task<Response.Device> Send(Request.GetDevice message)
        {
            return ScheduledSend<Response.Device>(message);
        }

        public Task<Response.Udp> Send(Request.GetUpdPushPort message)
        {
            return ScheduledSend<Response.Udp>(message);
        }

        public Task<Response.Udp> Send(Request.SetUdpPushPort message)
        {
            return ScheduledSend<Response.Udp>(message);
        }

        public Task<Response.Save> Send(Request.Save message)
        {
            return ScheduledSend<Response.Save>(message);
        }

        public void Open()
        {
            _connection = new CompositeDisposable(
                _responses.Connect(),
                _send.Subscribe()
            );
        }

        public void Close()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
