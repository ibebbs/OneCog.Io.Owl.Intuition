using FakeItEasy;
using Microsoft.Reactive.Testing;
using Nito.AsyncEx;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Subject = OneCog.Io.Owl.Intuition.Command;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Endpoint
{
    [TestFixture]
    public class InstanceShould
    {
        private static readonly IPEndPoint LocalEndpoint = new IPEndPoint(IPAddress.Loopback, 5432);
        private static readonly IPEndPoint RemoteEndpoint = new IPEndPoint(IPAddress.None, 5432);
        private static readonly string UdpKey = "TEST-UDP-KEY";

        private Socket.IFactory _socketFactory;
        private Subject.Response.IParser _responseParser;
        private Socket.IUdpClient _receiveSocket;
        private Socket.IUdpClient _sendSocket;
        private TestScheduler _scheduler;
        
        [SetUp]
        public void Setup()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            _scheduler = new TestScheduler();
            _socketFactory = A.Fake<Socket.IFactory>();

            _receiveSocket = A.Fake<Socket.IUdpClient>(x => x.Synchronized());
            _sendSocket = A.Fake<Socket.IUdpClient>(x => x.Synchronized());

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).Returns(_receiveSocket);
            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).Returns(_sendSocket);

            _responseParser = A.Fake<Subject.Response.IParser>();
        }

        [Test]
        public void ConstructReceiveSocketWhenOpened()
        {
            TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);

            Subject.Endpoint.Instance instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ConstuctSendSocketWhenOpened()
        {
            TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);
            
            Subject.Endpoint.Instance instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void SendBytesToRemoteEndpointWhenCommandIsSent()
        {
            TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();
            TaskCompletionSource<int> sendTask = new TaskCompletionSource<int>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);
            A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).Invokes(call => sendTask.SetResult(call.GetArgument<int>(1))).Returns(sendTask.Task);
            
            Subject.Endpoint.Instance instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey);

            instance.Open();

            var response = instance.Send(new Subject.Request.GetDevice(0));

            //await A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Within.TimeSpan(TimeSpan.FromSeconds(10)));

            A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ReceiveBytesFromRemoteEndpointWhenCommandIsSent()
        {
            byte[] buffer = Encoding.ASCII.GetBytes("TEST");

            TaskCompletionSource<UdpReceiveResult> receiveTask = null;
            TaskCompletionSource<int> sendTask = new TaskCompletionSource<int>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).ReturnsLazily(
                call =>
                {
                    receiveTask = new TaskCompletionSource<UdpReceiveResult>();
                    return receiveTask.Task;
                }
            );

            A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint))
             .Invokes(
                call =>
                {
                    sendTask.SetResult(call.GetArgument<int>(1));
                    receiveTask.SetResult(new UdpReceiveResult(buffer, RemoteEndpoint));
                })
             .Returns(sendTask.Task);

            Subject.Response.Device expected = new Subject.Response.Device(Subject.Status.Ok, 0, "TEST", "TEST", "TEST", Values.SignalStrength.Parse("0"), Values.LinkQuality.Parse("0"), Values.BatteryState.Parse("0"), TimeSpan.FromSeconds(1), 1, 1);

            A.CallTo(() => _responseParser.GetResponses("TEST")).Returns(new[] { expected });

            Subject.Endpoint.Instance instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            Task<Subject.Response.Device> responseTask = instance.Send(new Subject.Request.GetDevice(0));

            _scheduler.AdvanceBy(1);

            Subject.Response.Device actual = responseTask.Result;
            
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void OnlySendASingleCommandAtATime()
        {
            AsyncContext.Run(
                async () =>
                {
                    TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();
                    TaskCompletionSource<int> sendTask = new TaskCompletionSource<int>();

                    A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);
                    A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).Returns(sendTask.Task);

                    Subject.Endpoint.Instance instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey);

                    instance.Open();

                    A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustNotHaveHappened();

                    var deviceResponse = instance.Send(new Subject.Request.GetDevice(0));

                    await A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Within.TimeSpan(TimeSpan.FromSeconds(10)));

                    A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);

                    var versionResponse = instance.Send(new Subject.Request.GetVersion());

                    A.CallTo(() => _sendSocket.SendAsync(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
                }
            );
        }
    }
}
