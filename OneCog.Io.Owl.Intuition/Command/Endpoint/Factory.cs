
using OneCog.Io.Owl.Intuition.Socket;

namespace OneCog.Io.Owl.Intuition.Command.Endpoint
{
    public interface IFactory
    {
        IInstance CreateEndpoint(Device.Settings.IValues settings);
    }

    public class Factory : IFactory
    {
        public static readonly IFactory Default = new Factory(Socket.Factory.Default, Response.Parser.Default);

        private readonly Socket.IFactory _socketFactory;
        private readonly Response.IParser _responseParser;

        public Factory(Socket.IFactory socketFactory, Response.IParser responseParser)
        {
            _socketFactory = socketFactory;
            _responseParser = responseParser;
        }

        public IInstance CreateEndpoint(Device.Settings.IValues settings)
        {
            Instance endpoint = new Instance(_socketFactory, _responseParser, settings.LocalCommandEndpoint, settings.OwlCommandEndpoint, settings.OwlCommandResponseTimeout, settings.OwlCommandKey);

            Instrumentation.Command.Endpoint.CreatedAt(settings.LocalCommandEndpoint.Address.ToString(), settings.LocalCommandEndpoint.Port);

            return endpoint;
        }
    }
}
