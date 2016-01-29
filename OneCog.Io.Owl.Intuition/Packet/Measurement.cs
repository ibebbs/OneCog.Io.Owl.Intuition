using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Packet
{
    public class Measurement
    {
        [XmlAttribute("units")]
        public string Units { get; set; }

        [XmlText]
        public double Value { get; set; }
    }
}
