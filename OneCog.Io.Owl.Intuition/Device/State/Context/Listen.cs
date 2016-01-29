
namespace OneCog.Io.Owl.Intuition.Device.State.Context
{
    public interface IListen : IContext
    {
        Settings.IValues Settings { get; }

        Command.Endpoint.IInstance CommandEndpoint { get; }

        Values.Version Version { get; }
    }

    internal class Listen : IListen
    {
        public Listen(Command.Endpoint.IInstance commandEndpoint, Values.Version version, Settings.IValues settings)
        {
            CommandEndpoint = commandEndpoint;
            Version = version;
            Settings = settings;
        }

        public Command.Endpoint.IInstance CommandEndpoint { get; private set; }
        public Values.Version Version { get; private set; }
        public Settings.IValues Settings { get; private set; }
    }
}
