namespace OneCog.Io.Owl.Intuition.Packet.Endpoint
{
    public interface IFactory
    {
        IInstance CreateEndpoint(Device.Settings.IValues settings);
    }

    internal class Factory : IFactory
    {
        public static readonly IFactory Default = new Factory(new Parser());

        private readonly IParser _packetParser;

        public Factory(IParser packetParser)
        {
            _packetParser = packetParser;
        }

        public IInstance CreateEndpoint(Device.Settings.IValues settings)
        {
            return new Instance(_packetParser, settings.LocalPacketEndpoint);
        }
    }
}
