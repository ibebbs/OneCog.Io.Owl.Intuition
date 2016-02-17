using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Network.Packet
{
    [XmlRoot("electricity")]
    public class Electricity : IReading
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("signal", typeof(Signal))]
        public Signal Signal { get; set; }

        [XmlElement("battery", typeof(Battery))]
        public Battery Battery { get; set; }

        [XmlElement("chan", typeof(Channel))]
        public Channel[] Channels { get; set; }
    }
}
