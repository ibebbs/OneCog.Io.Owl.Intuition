using Reactive.EventAggregator;
using System;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    public interface IFactory
    {
        IState For(Context.IContext context);
    }

    internal class Factory : IFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ITransition _transition;
        private readonly Command.Endpoint.IFactory _commandEndpointFactory;
        private readonly Packet.Endpoint.IFactory _packetEndpointFactory;

        public Factory(IEventAggregator eventAggregator, ITransition transition, Command.Endpoint.IFactory commandEndpointFactory, Packet.Endpoint.IFactory packetEndpointFactory)
        {
            _eventAggregator = eventAggregator;
            _transition = transition;
            _commandEndpointFactory = commandEndpointFactory;
            _packetEndpointFactory = packetEndpointFactory;
        }

        private IState ForContext(Context.IDisconnected context)
        {
            return new Disconnected(_eventAggregator, _transition);
        }

        private IState ForContext(Context.IConnection context)
        {
            return new Connecting(_transition, _commandEndpointFactory, context);
        }

        private IState ForContext(Context.IConfiguration context)
        {
            return new Configuring(_transition, context);
        }

        private IState ForContext(Context.IListen context)
        {
            return new Listening(_eventAggregator, _transition, _packetEndpointFactory, context);
        }

        private IState ForContext(Context.IFault context)
        {
            return new Faulted(_eventAggregator, _transition, context);
        }

        private IState ForContext(Context.IContext context)
        {
            throw new InvalidOperationException(string.Format("Unable to determine state for context of type '{0}'", context.GetType().FullName));
        }

        public IState For(Context.IContext context)
        {
            dynamic dynamicContext = context;

            return ForContext(dynamicContext);
        }
    }
}
