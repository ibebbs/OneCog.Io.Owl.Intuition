﻿using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Network.Packet
{
    [XmlRoot("signal")]
    public class Signal
    {
        [XmlAttribute("rssi")]
        public int SignalStrength { get; set; }

        [XmlAttribute("lqi")]
        public int LinkQuality { get; set; }
    }
}
