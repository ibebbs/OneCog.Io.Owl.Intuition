using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net;
using Subject = OneCog.Io.Owl.Intuition;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Response
{
    [TestFixture]
    public class ParserTestFixture
    {
        private static readonly Subject.Network.Command.Response.Builder.DeviceResponse DeviceResponseBuilder = new Subject.Network.Command.Response.Builder.DeviceResponse();
        private static readonly Subject.Network.Command.Response.Builder.RostaResponse RostaResponseBuilder = new Subject.Network.Command.Response.Builder.RostaResponse();
        private static readonly Subject.Network.Command.Response.Builder.VersionResponse VersionResponseBuilder = new Subject.Network.Command.Response.Builder.VersionResponse();
        private static readonly Subject.Network.Command.Response.Builder.UdpResponse UdpResponseBuilder = new Subject.Network.Command.Response.Builder.UdpResponse();
        private static readonly Subject.Network.Command.Response.Builder.SaveResponse SaveResponseBuilder = new Subject.Network.Command.Response.Builder.SaveResponse();

        private static readonly IEnumerable<Subject.Network.Command.Response.IBuilder> Builders = new Subject.Network.Command.Response.IBuilder[]
        {
            DeviceResponseBuilder,
            RostaResponseBuilder,
            VersionResponseBuilder,
            UdpResponseBuilder,
            SaveResponseBuilder
        };

        [Test]
        public void ShouldParseVersionResponse()
        {
            Subject.Network.Command.Response.Parser subject = new Subject.Network.Command.Response.Parser(Builders);

            IEnumerable<Subject.Network.Command.IResponse> responses = subject.GetResponses(Resources.VersionResponse);
            Subject.Network.Command.Response.Version response = responses.OfType<Subject.Network.Command.Response.Version>().FirstOrDefault();

            Assert.IsNotNull(response);
            Assert.AreEqual(Subject.Network.Command.Status.Ok, response.Status);
            Assert.AreEqual("3", response.Firmware);
            Assert.AreEqual("1", response.Revision);
            Assert.AreEqual("4", response.Build);
        }

        [Test]
        public void ShouldParseRostaResponse()
        {
            Subject.Network.Command.Response.Parser subject = new Subject.Network.Command.Response.Parser(Builders);

            IEnumerable<Subject.Network.Command.IResponse> responses = subject.GetResponses(Resources.RostaResponse);
            Subject.Network.Command.Response.Rosta response = responses.OfType<Subject.Network.Command.Response.Rosta>().FirstOrDefault();

            Assert.IsNotNull(response);
            Assert.AreEqual(Subject.Network.Command.Status.Ok, response.Status);
            Assert.IsNotNull(response.Devices);            
            Assert.AreEqual("0=CMR180|1=ROOM_STAT", string.Join("|", response.Devices.Select(tuple => string.Format("{0}={1}", tuple.Item1, tuple.Item2)).ToArray()));
        }

        [Test]
        public void ShouldParseDeviceResponse()
        {
            Subject.Network.Command.Response.Parser subject = new Subject.Network.Command.Response.Parser(Builders);

            IEnumerable<Subject.Network.Command.IResponse> responses = subject.GetResponses(Resources.DeviceResponse);
            Subject.Network.Command.Response.Device response = responses.OfType<Subject.Network.Command.Response.Device>().FirstOrDefault();

            Assert.IsNotNull(response);
            Assert.AreEqual(Subject.Network.Command.Status.Ok, response.Status);
            Assert.AreEqual(0, response.Index);
            Assert.AreEqual("F2", response.DeviceAddress);
            Assert.AreEqual("CMR180", response.DeviceType);
            Assert.AreEqual("0", response.DeviceState);
            Assert.AreEqual(Values.SignalStrength.Parse("-53"), response.SignalStrength);
            Assert.AreEqual(Values.LinkQuality.Parse("20"), response.LinkQuality);
            Assert.AreEqual(Values.BatteryState.Parse("1"), response.BatteryState);
            Assert.AreEqual(TimeSpan.FromSeconds(1), response.TimeSinceLastPacketReceived);
            Assert.AreEqual(178, response.ReceivedPackets);
            Assert.AreEqual(0, response.SentPackets);
        }

        [Test]
        public void ShouldParseUdpResponse()
        {
            Subject.Network.Command.Response.Parser subject = new Subject.Network.Command.Response.Parser(Builders);

            IEnumerable<Subject.Network.Command.IResponse> responses = subject.GetResponses(Resources.UdpResponse);
            Subject.Network.Command.Response.Udp response = responses.OfType<Subject.Network.Command.Response.Udp>().FirstOrDefault();

            Assert.IsNotNull(response);
            Assert.AreEqual(Subject.Network.Command.Status.Ok, response.Status);
            Assert.AreEqual(string.Empty, response.HostName);
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("192.168.1.27"), 13), response.Endpoint);
        }

        [Test]
        public void ShouldParseSaveResponse()
        {
            Subject.Network.Command.Response.Parser subject = new Subject.Network.Command.Response.Parser(Builders);

            IEnumerable<Subject.Network.Command.IResponse> responses = subject.GetResponses(Resources.SaveResponse);
            Subject.Network.Command.Response.Save response = responses.OfType<Subject.Network.Command.Response.Save>().FirstOrDefault();

            Assert.IsNotNull(response);
            Assert.AreEqual(Subject.Network.Command.Status.Ok, response.Status);
        }
    }
}
