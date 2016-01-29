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
        private class ScheduledRequest
        {
            public ScheduledRequest(IRequest request, Func<IObservable<IResponse>, IDisposable> responseParser, Task completion)
            {
                Request = request;
                ResponseParser = responseParser;
                Completion = completion;
            }

            public IRequest Request { get; private set; }

            public Func<IObservable<IResponse>, IDisposable> ResponseParser { get; private set; }

            public Task Completion { get; private set; }
        }

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
                        .FromAsync(udpReceive.ReceiveAsync).Repeat()
                        .Select(result => result.Buffer)
                        .Select(Encoding.ASCII.GetString)
                        .Do(Instrumentation.Command.Endpoint.Receive)
                        .SelectMany(responseParser.GetResponses)
                        .Do(Instrumentation.Command.Endpoint.Response))
                .Publish();

            _send = Observable
                .Using(
                    () => socketFactory.ConstructCommandSendSocket(localEndpoint),
                    udpSend => _sendQueue
                        .Select(scheduledRequest => new { Socket = udpSend, ScheduledRequest = scheduledRequest }))
                        .ObserveOn(_scheduler)
                        .SelectMany(tuple => Send(tuple.Socket, tuple.ScheduledRequest));                
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

        private async Task<ScheduledRequest> Send(Socket.IUdpClient socket, ScheduledRequest scheduledRequest)
        {
            Instrumentation.Command.Endpoint.Request(scheduledRequest.Request);

            string requestString = AddKey(scheduledRequest.Request.AsString());

            Instrumentation.Command.Endpoint.Send(requestString);

            byte[] datagram = Encoding.ASCII.GetBytes(requestString);

            using (scheduledRequest.ResponseParser(_responses))
            {
                await socket.SendAsync(datagram, datagram.Length, _remoteEndpoint);

                await scheduledRequest.Completion;
            }

            return scheduledRequest;
        }
        
        private Task<T> ScheduledSend<T>(IRequest request) where T : IResponse
        {
            TaskCompletionSource<T> result = new TaskCompletionSource<T>();

            Func<IObservable<IResponse>, IDisposable> handler = responses => responses
                .OfType<T>()
                .ThrowWhen(RequestFailed, response => RequestException(request, response))
                .Timeout(_requestTimeout)
                .Take(1)
                .Subscribe(result.SetResult, result.SetException);

            _sendQueue.OnNext(new ScheduledRequest(request, handler, result.Task));

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
