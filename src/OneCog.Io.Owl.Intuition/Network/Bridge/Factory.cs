namespace OneCog.Io.Owl.Intuition.Network.Bridge
{
    public interface IFactory
    {
        IBridge Create(Settings.IValues settings);
    }

    public class Factory : IFactory
    {
        public static readonly IFactory Default = new Factory(Command.Endpoint.Factory.Default, Packet.Endpoint.Factory.Default);

        private readonly Command.Endpoint.IFactory _commandEndpointFactory;
        private readonly Packet.Endpoint.IFactory _packetEndpointFactory;

        public Factory(Command.Endpoint.IFactory commandEndpointFactory, Packet.Endpoint.IFactory packetEndpointFactory)
        {
            _commandEndpointFactory = commandEndpointFactory;
            _packetEndpointFactory = packetEndpointFactory;
        }

        public IBridge Create(Settings.IValues settings)
        {
            return new Instance(settings, _commandEndpointFactory, _packetEndpointFactory);
        }
    }
}
