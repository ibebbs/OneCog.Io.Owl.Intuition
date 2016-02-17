using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Network.Packet
{
    [XmlRoot("chan")]
    public class Channel
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("curr", typeof(Measurement))]
        public Measurement Current { get; set; }

        [XmlElement("day", typeof(Measurement))]
        public Measurement Day { get; set; }
    }
}
