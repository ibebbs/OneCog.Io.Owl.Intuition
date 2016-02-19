using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace OneCog.Io.Owl.Intuition.Console
{
    public class SettingsProvider : Settings.IProvider
    {
        private readonly Options _options;

        public SettingsProvider(Options options)
        {
            _options = options;
        }

        public Settings.IValues GetValues()
        {
            IPAddress ipAddress = string.IsNullOrWhiteSpace(_options.LocalIpAddress)
              ? Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
              : IPAddress.Parse(_options.LocalIpAddress);

            return new Settings.Values(
                new IPEndPoint(ipAddress, _options.LocalCommandPort),
                new IPEndPoint(ipAddress, _options.LocalPacketPort),
                _options.OwlUdpKey,
                PhysicalAddress.Parse(_options.OwlMacAddress),
                new IPEndPoint(IPAddress.Parse(_options.OwlIpAddress), _options.OwlCommandPort),
                TimeSpan.FromSeconds(10),
                true);
        }
    }
}
