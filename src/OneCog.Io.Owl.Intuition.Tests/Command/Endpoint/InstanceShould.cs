using FakeItEasy;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Subject = OneCog.Io.Owl.Intuition;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Endpoint
{
    [TestFixture]
    public class InstanceShould
    {
        private static readonly IPEndPoint LocalEndpoint = new IPEndPoint(IPAddress.Loopback, 5432);
        private static readonly IPEndPoint RemoteEndpoint = new IPEndPoint(IPAddress.None, 5432);
        private static readonly string UdpKey = "TEST-UDP-KEY";

        private Subject.Network.Socket.IFactory _socketFactory;
        private Subject.Network.Command.Response.IParser _responseParser;
        private Subject.Network.Socket.IObservableUdpClient _receiveSocket;
        private Subject.Network.Socket.IObservableUdpClient _sendSocket;
        private TestScheduler _scheduler;
        
        [SetUp]
        public void Setup()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            _scheduler = new TestScheduler();
            _socketFactory = A.Fake<Subject.Network.Socket.IFactory>();

            _receiveSocket = A.Fake<Subject.Network.Socket.IObservableUdpClient>();
            _sendSocket = A.Fake<Subject.Network.Socket.IObservableUdpClient>();

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).Returns(_receiveSocket);
            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).Returns(_sendSocket);

            _responseParser = A.Fake<Subject.Network.Command.Response.IParser>();
        }

        [Test]
        public void ConstructReceiveSocketWhenOpened()
        {
            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ConstuctSendSocketWhenOpened()
        {            
            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void SendBytesToRemoteEndpointWhenCommandIsSent()
        {            
            A.CallTo(() => _sendSocket.Send(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).ReturnsLazily(call => Observable.Return(call.GetArgument<int>(1)));
            
            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            var response = instance.Send(new Subject.Network.Command.Request.GetDevice(0));

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ReceiveBytesFromRemoteEndpointWhenCommandIsSent()
        {
            byte[] buffer = Encoding.ASCII.GetBytes("TEST");
            Subject.Network.Command.Response.Device expected = new Subject.Network.Command.Response.Device(Subject.Network.Command.Status.Ok, 0, "TEST", "TEST", "TEST", Values.SignalStrength.Parse("0"), Values.LinkQuality.Parse("0"), Values.BatteryState.Parse("0"), TimeSpan.FromSeconds(1), 1, 1);
            A.CallTo(() => _responseParser.GetResponses("TEST")).Returns(new[] { expected });

            Subject<UdpReceiveResult> received = new Subject<UdpReceiveResult>();

            A.CallTo(() => _receiveSocket.Receive()).ReturnsLazily(call => received);
            A.CallTo(() => _sendSocket.Send(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).ReturnsLazily(call => Observable.Return(call.GetArgument<int>(1)));

            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            Task<Subject.Network.Command.Response.Device> responseTask = instance.Send(new Subject.Network.Command.Request.GetDevice(0));

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _receiveSocket.Receive()).MustHaveHappened();

            received.OnNext(new UdpReceiveResult(buffer, RemoteEndpoint));

            Assert.That(responseTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));            
            Assert.That(responseTask.Result, Is.EqualTo(expected));
        }

        [Test]
        public void OnlySendASingleCommandAtATime()
        {
            A.CallTo(() => _receiveSocket.Receive()).ReturnsLazily(call => Observable.Never<UdpReceiveResult>());
            A.CallTo(() => _sendSocket.Send(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).ReturnsLazily(call => Observable.Return(call.GetArgument<int>(1)));

            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustNotHaveHappened();

            var deviceResponse = instance.Send(new Subject.Network.Command.Request.GetDevice(0));

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);

            var versionResponse = instance.Send(new Subject.Network.Command.Request.GetVersion());

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void QueuedCommandsAreSentInSequence()
        {
            byte[] deviceResponse = Encoding.ASCII.GetBytes("DEVICE");
            Subject.Network.Command.Response.Device expectedDevice = new Subject.Network.Command.Response.Device(Subject.Network.Command.Status.Ok, 0, "TEST", "TEST", "TEST", Values.SignalStrength.Parse("0"), Values.LinkQuality.Parse("0"), Values.BatteryState.Parse("0"), TimeSpan.FromSeconds(1), 1, 1);
            A.CallTo(() => _responseParser.GetResponses("DEVICE")).Returns(new[] { expectedDevice });

            byte[] versionResponse = Encoding.ASCII.GetBytes("VERSION");
            Subject.Network.Command.Response.Version expectedVersion = new Subject.Network.Command.Response.Version(Subject.Network.Command.Status.Ok, "TEST", "TEST", "TEST");
            A.CallTo(() => _responseParser.GetResponses("VERSION")).Returns(new[] { expectedVersion });

            Subject<UdpReceiveResult> received = new Subject<UdpReceiveResult>();
            
            A.CallTo(() => _receiveSocket.Receive()).Returns(received);
            A.CallTo(() => _sendSocket.Send(A<byte[]>.Ignored, A<int>.Ignored, RemoteEndpoint)).ReturnsLazily(call => Observable.Return(call.GetArgument<int>(1)));

            Subject.Network.Command.Endpoint.Instance instance = new Subject.Network.Command.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);

            instance.Open();

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustNotHaveHappened();
            
            Task<Subject.Network.Command.Response.Device> deviceTask = instance.Send(new Subject.Network.Command.Request.GetDevice(0));
            Task<Subject.Network.Command.Response.Version> versionTask = instance.Send(new Subject.Network.Command.Request.GetVersion());

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Once);

            received.OnNext(new UdpReceiveResult(deviceResponse, RemoteEndpoint));

            Assert.That(deviceTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(expectedDevice, Is.EqualTo(deviceTask.Result));

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Twice);

            received.OnNext(new UdpReceiveResult(versionResponse, RemoteEndpoint));

            Assert.That(versionTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(expectedVersion, Is.EqualTo(versionTask.Result));

            _scheduler.AdvanceBy(1);

            A.CallTo(() => _sendSocket.Send(A<byte[]>.That.Matches(bytes => bytes.Length > 0), A<int>.Ignored, RemoteEndpoint)).MustHaveHappened(Repeated.Exactly.Twice);

        }
    }
}
