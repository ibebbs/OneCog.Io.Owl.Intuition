using System.Net;

namespace OneCog.Io.Owl.Intuition.Device.State.Context
{
    public interface IConnection : IContext
    {
        Settings.IValues Settings { get; }
    }

    internal class Connection : IConnection
    {
        public Connection(Settings.IValues settings)
        {
            Settings = settings;
        }

        public Settings.IValues Settings { get; private set; }
    }
}
