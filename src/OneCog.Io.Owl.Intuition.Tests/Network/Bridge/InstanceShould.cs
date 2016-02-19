using FakeItEasy;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using System;
using System.Reactive.Subjects;
using Subject = OneCog.Io.Owl.Intuition;
using System.Net;

namespace OneCog.Io.Owl.Intuition.Tests.Network.Bridge
{
    [TestFixture]
    public class InstanceShould
    {
        private Settings.IValues _settings;
        private Subject.Network.Command.Endpoint.IFactory _commandEndpointFactory;
        private Subject.Network.Packet.Endpoint.IFactory _packetEndpointFactory;
        private TestScheduler _commandScheduler;

        [SetUp]
        public void Setup()
        {
            _settings = A.Fake<Settings.IValues>();
            _commandEndpointFactory = A.Fake<Subject.Network.Command.Endpoint.IFactory>();
            _packetEndpointFactory = A.Fake<Subject.Network.Packet.Endpoint.IFactory>();
            _commandScheduler = new TestScheduler();
        }

        [Test]
        public void CreatePacketEndpointWhenFirstSubscriptionMade()
        {
            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            using (subject.Readings.Subscribe())
            {
                A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Test]
        public void NotCreatePacketEndpointWhenSubsequentSubscriptionsMade()
        {
            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            using (subject.Readings.Subscribe())
            {
                using (subject.Readings.Subscribe())
                {
                    A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
                }
            }
        }

        [Test]
        public void DisposePacketEndpointWhenFirstSubscriptionDisposed()
        {
            Subject.Network.Packet.Endpoint.IInstance packetEndpoint = A.Fake<Subject.Network.Packet.Endpoint.IInstance>();
            A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).Returns(packetEndpoint);

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            using (subject.Readings.Subscribe())
            {
                A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }

            A.CallTo(() => packetEndpoint.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void DisposePacketEndpointWhenLastSubscriptionDisposed()
        {
            Subject.Network.Packet.Endpoint.IInstance packetEndpoint = A.Fake<Subject.Network.Packet.Endpoint.IInstance>();
            A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).Returns(packetEndpoint);

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            using (subject.Readings.Subscribe())
            {
                using (subject.Readings.Subscribe())
                {
                    A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
                }

                A.CallTo(() => packetEndpoint.Dispose()).MustNotHaveHappened();
            }

            A.CallTo(() => packetEndpoint.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }
        
        [Test]
        public void CallSendOnCommandEndpointWhenGetDeviceRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);
            
            Subject.Network.Command.Request.GetDevice request = new Subject.Network.Command.Request.GetDevice(0);
            subject.Send(request);
            
            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CallSendOnCommandEndpointWhenGetVersionRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            Subject.Network.Command.Request.GetVersion request = new Subject.Network.Command.Request.GetVersion();
            subject.Send(request);

            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CallSendOnCommandEndpointWhenGetRostaRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            Subject.Network.Command.Request.GetRosta request = new Subject.Network.Command.Request.GetRosta();
            subject.Send(request);

            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public void CallSendOnCommandEndpointWhenGetUpdPushPortRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            Subject.Network.Command.Request.GetUpdPushPort request = new Subject.Network.Command.Request.GetUpdPushPort();
            subject.Send(request);

            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public void CallSendOnCommandEndpointWhenSetUpdPushPortRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            Subject.Network.Command.Request.SetUdpPushPort request = new Subject.Network.Command.Request.SetUdpPushPort(new IPEndPoint(IPAddress.Loopback, 1234));
            subject.Send(request);

            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public void CallSeveOnCommandEndpointWhenSaveRequestReceived()
        {
            Subject.Network.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Network.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Network.Command.IRequest> requests = new Subject<Subject.Network.Command.IRequest>();

            Subject.Network.Bridge.Instance subject = new Subject.Network.Bridge.Instance(_settings, _commandEndpointFactory, _packetEndpointFactory);

            Subject.Network.Command.Request.Save request = new Subject.Network.Command.Request.Save();
            subject.Send(request);

            A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
