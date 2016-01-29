using FakeItEasy;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
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

        private Subject.Endpoint.Instance _instance;

        [SetUp]
        public void Setup()
        {
            _scheduler = new TestScheduler();
            _socketFactory = A.Fake<Socket.IFactory>();

            _receiveSocket = A.Fake<Socket.IUdpClient>();
            _sendSocket = A.Fake<Socket.IUdpClient>();

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).Returns(_receiveSocket);
            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).Returns(_sendSocket);

            _responseParser = A.Fake<Subject.Response.IParser>();

            _instance = new Subject.Endpoint.Instance(_socketFactory, _responseParser, LocalEndpoint, RemoteEndpoint, TimeSpan.FromSeconds(10), UdpKey, _scheduler);
        }

        [Test]
        public void ConstructReceiveSocketWhenOpened()
        {
            TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);

            _instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandReceiveSocket(LocalEndpoint)).MustHaveHappened();
        }

        [Test]
        public void ConstuctSendSocketWhenOpened()
        {
            TaskCompletionSource<UdpReceiveResult> receiveTask = new TaskCompletionSource<UdpReceiveResult>();

            A.CallTo(() => _receiveSocket.ReceiveAsync()).Returns(receiveTask.Task);

            _instance.Open();

            A.CallTo(() => _socketFactory.ConstructCommandSendSocket(LocalEndpoint)).MustHaveHappened();
        }
    }
}
