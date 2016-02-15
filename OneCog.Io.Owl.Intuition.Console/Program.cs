using Humanizer;
using Nito.AsyncEx;
using OneCog.Io.Owl.Intuition.Device;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
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
                Device.Type deviceType;

                if (Device.Type.TryParse(options.OwlDeviceType, out deviceType))
                {
                    Device.Settings.IProvider settingsProvider = new SettingsProvider(options);

                    Device.IInstance instance = Device.Factory.Default.Create(deviceType, settingsProvider);

                    AsyncContext.Run(() => Loop(settingsProvider, instance));
                }
                else
                {
                    WriteLine(options.GetUsage());

                    WriteLine("Unknown device type: '{0}'", options.OwlDeviceType);
                    WriteLine("Device type must be one of the following:");

                    foreach (string typeName in Device.Type.KnownTypes.Select(knownType => knownType.Name))
                    {
                        WriteLine("  {0}", typeName);
                    }
                }
            }
            else
            {
                WriteLine(options.GetUsage());
            }
        }

        private static Task<T> Request<T>(IObserver<Command.IRequest> requests, IObservable<Command.IResponse> responses, Command.IRequest request, int maxAttempts = 5)
        {
            Task<T> response = responses.OfType<T>().Take(1).Retry(maxAttempts).ToTask();

            requests.OnNext(request);

            return response;
        }

        private static Task ForReturn()
        {
            return Task.Run(() => ReadLine());
        }

        private static async Task Loop(Device.Settings.IProvider settingsProvider, Device.IInstance instance)
        {
            Device.Settings.IValues settings = settingsProvider.GetValues();

            Subject<Command.IRequest> requests = new Subject<Command.IRequest>();
            IConnectableObservable<Command.IResponse> responses = requests.SendTo(instance).Publish();

            using (instance.Readings.OfType<Packet.Electricity>().Subscribe(reading => WriteLine("Received Electricity Reading: {0}{1}. Total today: {2}{3}", reading.Channels[0].Current.Value, reading.Channels[0].Current.Units, reading.Channels[0].Day.Value, reading.Channels[0].Day.Units)))
            {
                using (responses.IgnoreElements().Subscribe(_ => { }, ex => WriteLine($"Exception: {ex}")))
                {
                    WriteLine("Connecting to Owl at IP address: {0}", settings.OwlCommandEndpoint.ToString());

                    using (responses.Connect())
                    {
                        Command.Response.Version version = await Request<Command.Response.Version>(requests, responses, new Command.Request.GetVersion());

                        WriteLine("Owl Firmware: {0}", version.Firmware);
                        WriteLine("Owl Revision: {0}", version.Revision);
                        WriteLine("Owl Build: {0}", version.Build);

                        Command.Response.Rosta rosta = await Request<Command.Response.Rosta>(requests, responses, new Command.Request.GetRosta());

                        WriteLine("Owl connected to {0}:", "device".ToQuantity(rosta.Devices.Count()));

                        foreach (Tuple<int, string> device in rosta.Devices)
                        {
                            WriteLine("  Device {0}: {1}", device.Item1, device.Item2);
                        }

                        Command.Response.Udp getUdp = await Request<Command.Response.Udp>(requests, responses, new Command.Request.GetUpdPushPort());

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

                            Command.Response.Udp setUdp = await Request<Command.Response.Udp>(requests, responses, new Command.Request.SetUdpPushPort(settings.LocalPacketEndpoint));

                            if (!string.IsNullOrWhiteSpace(setUdp.HostName))
                            {
                                WriteLine("Owl pushing pakcets to {0} ({1})", setUdp.HostName, setUdp.Endpoint);
                            }
                            else
                            {
                                WriteLine("Owl pushing pakcets to {0}", setUdp.Endpoint);
                            }

                            Command.Response.Save save = await Request<Command.Response.Save>(requests, responses, new Command.Request.Save());

                            WriteLine("Packet Endpoint Set");
                        }

                        WriteLine("Waiting for packets. Hit return to exit.");

                        await ForReturn();
                    }
                }
            }
        } 
    }
}
