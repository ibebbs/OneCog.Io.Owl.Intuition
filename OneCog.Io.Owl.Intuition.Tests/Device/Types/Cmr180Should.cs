using FakeItEasy;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Subject = OneCog.Io.Owl.Intuition;
using OneCog.Io.Owl.Intuition.Device;
using System.Net;

namespace OneCog.Io.Owl.Intuition.Tests.Device.Types
{
    [TestFixture]
    public class Cmr180Should
    {
        private Subject.Device.Settings.IValues _settings;
        private Subject.Command.Endpoint.IFactory _commandEndpointFactory;
        private Subject.Packet.Endpoint.IFactory _packetEndpointFactory;
        private TestScheduler _commandScheduler;

        [SetUp]
        public void Setup()
        {
            _settings = A.Fake<Subject.Device.Settings.IValues>();
            _commandEndpointFactory = A.Fake<Subject.Command.Endpoint.IFactory>();
            _packetEndpointFactory = A.Fake<Subject.Packet.Endpoint.IFactory>();
            _commandScheduler = new TestScheduler();
        }

        [Test]
        public void CreatePacketEndpointWhenFirstSubscriptionMade()
        {
            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (subject.Readings.Subscribe())
            {
                A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Test]
        public void NotCreatePacketEndpointWhenSubsequentSubscriptionsMade()
        {
            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

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
            Subject.Packet.Endpoint.IInstance packetEndpoint = A.Fake<Subject.Packet.Endpoint.IInstance>();
            A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).Returns(packetEndpoint);
            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (subject.Readings.Subscribe())
            {
                A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }

            A.CallTo(() => packetEndpoint.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void DisposePacketEndpointWhenLastSubscriptionDisposed()
        {
            Subject.Packet.Endpoint.IInstance packetEndpoint = A.Fake<Subject.Packet.Endpoint.IInstance>();
            A.CallTo(() => _packetEndpointFactory.CreateEndpoint(_settings)).Returns(packetEndpoint);
            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

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
        public void CreateCommandEndpointWhenFirstSubscriptionMade()
        {
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Test]
        public void NotCreateCommandEndpointWhenSubsequentSubscriptionsMade()
        {
            Subject<Subject.Command.IRequest> requestSourceA = new Subject<Intuition.Command.IRequest>();
            Subject<Subject.Command.IRequest> requestSourceB = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requestSourceA.SendTo(subject).Subscribe())
            {
                using (requestSourceB.SendTo(subject).Subscribe())
                {
                    A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
                }
            }
        }

        [Test]
        public void DisposeCommandEndpointWhenFirstSubscriptionDisposed()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
            }

            A.CallTo(() => commandEndpoint.Dispose()).MustHaveHappened();
        }

        [Test]
        public void DisposeCommandEndpointWhenLastSubscriptionDisposed()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);

            Subject<Subject.Command.IRequest> requestSourceA = new Subject<Intuition.Command.IRequest>();
            Subject<Subject.Command.IRequest> requestSourceB = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requestSourceA.SendTo(subject).Subscribe())
            {
                using (requestSourceB.SendTo(subject).Subscribe())
                {
                    A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).MustHaveHappened(Repeated.Exactly.Once);
                }

                A.CallTo(() => commandEndpoint.Dispose()).MustNotHaveHappened();
            }

            A.CallTo(() => commandEndpoint.Dispose()).MustHaveHappened();
        }

        [Test]
        public void CallSendOnCommandEndpointWhenGetDeviceRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.GetDevice request = new Subject.Command.Request.GetDevice(0);
                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Test]
        public void CallSendOnCommandEndpointWhenGetVersionRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.GetVersion request = new Subject.Command.Request.GetVersion();

                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Test]
        public void CallSendOnCommandEndpointWhenGetRostaRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.GetRosta request = new Subject.Command.Request.GetRosta();

                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }


        [Test]
        public void CallSendOnCommandEndpointWhenGetUpdPushPortRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.GetUpdPushPort request = new Subject.Command.Request.GetUpdPushPort();

                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }


        [Test]
        public void CallSendOnCommandEndpointWhenSetUpdPushPortRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.SetUdpPushPort request = new Subject.Command.Request.SetUdpPushPort(new IPEndPoint(IPAddress.Loopback, 1234));

                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }


        [Test]
        public void CallSendOnCommandEndpointWhenSaveRequestReceived()
        {
            Subject.Command.Endpoint.IInstance commandEndpoint = A.Fake<Subject.Command.Endpoint.IInstance>();
            A.CallTo(() => _commandEndpointFactory.CreateEndpoint(_settings)).Returns(commandEndpoint);
            Subject<Subject.Command.IRequest> requests = new Subject<Intuition.Command.IRequest>();

            Subject.Device.Types.Cmr180 subject = new Subject.Device.Types.Cmr180(_settings, _commandEndpointFactory, _packetEndpointFactory, _commandScheduler);

            using (requests.SendTo(subject).Subscribe())
            {
                Subject.Command.Request.Save request = new Subject.Command.Request.Save();

                requests.OnNext(request);

                _commandScheduler.AdvanceBy(1);

                A.CallTo(() => commandEndpoint.Send(request)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }
    }
}
