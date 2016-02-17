using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Network.Bridge
{
    internal class Instance : IBridge
    {
        private readonly IObservable<Packet.IReading> _readings;
        private Lazy<Command.Endpoint.IInstance> _commands;

        public Instance(Settings.IValues settings, Command.Endpoint.IFactory commandEndpointFactory, Packet.Endpoint.IFactory packetEndpointFactory)
        {
            _commands = new Lazy<Command.Endpoint.IInstance>(() => CreateCommandEndpoint(commandEndpointFactory, settings));

            _readings = Observable
                .Using(() => CreatePacketEndpoint(packetEndpointFactory, settings), endpoint => endpoint.Readings)
                .Publish()
                .RefCount();
        }

        public void Dispose()
        {
            if (_commands.IsValueCreated)
            {
                _commands.Value.Close();
                _commands.Value.Dispose();
                _commands = null;
            }
        }

        private Packet.Endpoint.IInstance CreatePacketEndpoint(Packet.Endpoint.IFactory packetEndpointFactory, Settings.IValues settings)
        {
            Packet.Endpoint.IInstance packetEndpoint = packetEndpointFactory.CreateEndpoint(settings);

            packetEndpoint.Open();

            return packetEndpoint;
        }

        private Command.Endpoint.IInstance CreateCommandEndpoint(Command.Endpoint.IFactory commandEndpointFactory, Settings.IValues settings)
        {
            Command.Endpoint.IInstance commandEndpoint = commandEndpointFactory.CreateEndpoint(settings);

            commandEndpoint.Open();

            return commandEndpoint;
        }

        public Task<Command.Response.Version> Send(Command.Request.GetVersion request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public Task<Command.Response.Rosta> Send(Command.Request.GetRosta request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public Task<Command.Response.Device> Send(Command.Request.GetDevice request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public Task<Command.Response.Udp> Send(Command.Request.GetUpdPushPort request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public Task<Command.Response.Udp> Send(Command.Request.SetUdpPushPort request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public Task<Command.Response.Save> Send(Command.Request.Save request)
        {
            if (_commands == null) throw new ObjectDisposedException("commands");

            return _commands.Value.Send(request);
        }

        public IObservable<Packet.IReading> Readings
        {
            get { return _readings; }
        }
    }
}
