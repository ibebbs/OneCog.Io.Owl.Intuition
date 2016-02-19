using System;
using NUnit.Framework;

namespace OneCog.Io.Owl.Intuition.Tests.Packet
{
    [TestFixture]
    public class SerializerTestFixture
    {
        [Test]
        public void ShouldCorrectlyDeserializeElectricityPacket()
        {
            Owl.Intuition.Network.Packet.Serializer subject = new Intuition.Network.Packet.Serializer();

            Owl.Intuition.Network.Packet.Electricity packet = subject.DeserializeElectricity(Resources.ElectricityPacket);

            Assert.IsNotNull(packet);
            Assert.AreEqual("4437190032EC", packet.Id);

            Assert.IsNotNull(packet.Signal);
            Assert.AreEqual(-53, packet.Signal.SignalStrength);
            Assert.AreEqual(19, packet.Signal.LinkQuality);

            Assert.IsNotNull(packet.Battery);
            Assert.AreEqual("100%", packet.Battery.Level);

            Assert.IsNotNull(packet.Channels);
            Assert.AreEqual(3, packet.Channels.Length);

            Assert.IsNotNull(packet.Channels[0]);
            Assert.AreEqual(0, packet.Channels[0].Id);

            Assert.IsNotNull(packet.Channels[0].Current);
            Assert.AreEqual("w", packet.Channels[0].Current.Units);
            Assert.AreEqual(1352.00, packet.Channels[0].Current.Value);

            Assert.IsNotNull(packet.Channels[0].Day);
            Assert.AreEqual("wh", packet.Channels[0].Day.Units);
            Assert.AreEqual(3308.63, packet.Channels[0].Day.Value);
        }
    }
}
