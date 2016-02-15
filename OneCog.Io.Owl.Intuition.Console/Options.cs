using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Console
{
    public class Options
    {
        [Option('r', "owlIpAddress", Required = true, HelpText = "IP address of the Owl Intuition Bridge, i.e. 192.168.1.55" )]
        public string OwlIpAddress { get; set; }

        [Option('m', "owlMacAddress", Required = true, HelpText = "MAC address of the Owl Intuition Bridge, i.e. 9E-D4-52-D2-DE-56")]
        public string OwlMacAddress { get; set; }

        [Option('t', "ownDeviceType", Required = true, HelpText = "Owl Device Type, i.e. CMR180")]
        public string OwlDeviceType { get; set; }

        [Option('k', "owlUdpKey", Required = true, HelpText = "Hexadecimal UDP Key for the Owl Intuition Bridge, i.e. 7BA6E1F2")]
        public string OwlUdpKey { get; set; }

        [Option("owlCommandPort", Required = false, DefaultValue = 5100, HelpText = "Command port for the Owl Intuition Bridge, i.e. 5100")]
        public int OwlCommandPort { get; set; }

        [Option("localIpAddress", Required = false, HelpText = "IP address of the local machine, i.e. 192.168.1.1")]
        public string LocalIpAddress { get; set; }

        [Option("localCommandPort", Required = false, DefaultValue = 5100, HelpText = "Command port for the local machine, i.e. 5100")]
        public int LocalCommandPort { get; set; }
            
        [Option("localPacketPort", Required = false, DefaultValue = 5110, HelpText = "Packet port for the local machine, i.e. 5110")]
        public int LocalPacketPort { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("Owl Intuition Console App 1.0");
            usage.AppendLine(HelpText.AutoBuild(this));
            return usage.ToString();
        }
    }
}
