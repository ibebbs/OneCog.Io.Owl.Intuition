using Humanizer;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static System.Console;


namespace OneCog.Io.Owl.Intuition.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Settings.IProvider settingsProvider = new SettingsProvider(options);
                Settings.IValues settingsValues = settingsProvider.GetValues();

                Network.IBridge instance = Network.Bridge.Factory.Default.Create(settingsValues);

                AsyncContext.Run(() => Loop(settingsValues, instance));
            }
            else
            {
                WriteLine(options.GetUsage());
            }
        }

        private static Task ForReturn()
        {
            return Task.Run(() => ReadLine());
        }

        private static async Task Loop(Settings.IValues settings, Network.IBridge instance)
        {
            using (instance.Readings.OfType<Network.Packet.Electricity>().Subscribe(reading => WriteLine("Received Electricity Reading: {0}{1}. Total today: {2}{3}", reading.Channels[0].Current.Value, reading.Channels[0].Current.Units, reading.Channels[0].Day.Value, reading.Channels[0].Day.Units)))
            {
                WriteLine("Connecting to Owl at IP address: {0}", settings.OwlCommandEndpoint.ToString());

                Network.Command.Response.Version version = await instance.Send(new Network.Command.Request.GetVersion());

                WriteLine("Owl Firmware: {0}", version.Firmware);
                WriteLine("Owl Revision: {0}", version.Revision);
                WriteLine("Owl Build: {0}", version.Build);

                Network.Command.Response.Rosta rosta = await instance.Send(new Network.Command.Request.GetRosta());

                WriteLine("Owl connected to {0}:", "device".ToQuantity(rosta.Devices.Count()));

                foreach (Tuple<int, string> device in rosta.Devices)
                {
                    WriteLine("  Device {0}: {1}", device.Item1, device.Item2);
                }

                Network.Command.Response.Udp getUdp = await instance.Send(new Network.Command.Request.GetUpdPushPort());

                if (!string.IsNullOrWhiteSpace(getUdp.HostName))
                {
                    WriteLine("Owl pushing pakcets to {0} ({1})", getUdp.HostName, getUdp.Endpoint);
                }
                else
                {
                    WriteLine("Owl pushing pakcets to {0}", getUdp.Endpoint);
                }

                if (!getUdp.Endpoint.Equals(settings.LocalPacketEndpoint))
                {
                    WriteLine("Settings Packet Endpoint");

                    Network.Command.Response.Udp setUdp = await instance.Send(new Network.Command.Request.SetUdpPushPort(settings.LocalPacketEndpoint));

                    if (!string.IsNullOrWhiteSpace(setUdp.HostName))
                    {
                        WriteLine("Owl pushing pakcets to {0} ({1})", setUdp.HostName, setUdp.Endpoint);
                    }
                    else
                    {
                        WriteLine("Owl pushing pakcets to {0}", setUdp.Endpoint);
                    }

                    Network.Command.Response.Save save = await instance.Send(new Network.Command.Request.Save());

                    WriteLine("Packet Endpoint Set");
                }

                WriteLine("Waiting for packets. Hit return to exit.");

                await ForReturn();
            }
        } 
    }
}
