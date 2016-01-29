using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Packet
{
    [XmlRoot("battery")]
    public class Battery
    {
        [XmlAttribute("level")]
        public string Level { get; set; }
    }
}
