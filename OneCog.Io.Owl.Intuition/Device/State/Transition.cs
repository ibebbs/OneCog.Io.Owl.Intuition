using Reactive.EventAggregator;
using System;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    public interface ITransition
    {
        void ToDisconnected();
        void ToConnecting();
        void ToConfiguration(Command.Endpoint.IInstance commandEndpoint, Values.Version version);
        void ToListening(Command.Endpoint.IInstance commandEndpoint, Values.Version version);
        void ToFaulted(Command.Endpoint.IInstance commandEndpoint, Exception exception);
    }

    internal class Transition : ITransition
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Event.IFactory _eventFactory;
        private readonly Context.IFactory _contextFactory;

        public Transition(IEventAggregator eventAggregator, Event.IFactory eventFactory, Context.IFactory contextFactory)
        {
            _eventAggregator = eventAggregator;
            _eventFactory = eventFactory;
            _contextFactory = contextFactory;
        }

        public void ToDisconnected()
        {
            Context.IDisconnected context = _contextFactory.ForDisconnected();
            Event.Transition message = _eventFactory.ForStateTransition(context);

            _eventAggregator.Publish(message);
        }

        public void ToConnecting()
        {
            Context.IConnection context = _contextFactory.ForConnection();
            Event.Transition message = _eventFactory.ForStateTransition(context);

            _eventAggregator.Publish(message);
        }

        public void ToConfiguration(Command.Endpoint.IInstance commandEndpoint, Values.Version version)
        {
            Context.IConfiguration context = _contextFactory.ForConfiguration(commandEndpoint, version);
            Event.Transition message = _eventFactory.ForStateTransition(context);

            _eventAggregator.Publish(message);
        }
        
        public void ToListening(Command.Endpoint.IInstance commandEndpoint, Values.Version version)
        {
            Context.IListen context = _contextFactory.ForListen(commandEndpoint, version);
            Event.Transition message = _eventFactory.ForStateTransition(context);

            _eventAggregator.Publish(message);
        }

        public void ToFaulted(Command.Endpoint.IInstance commandEndpoint, Exception exception)
        {
            Context.IFault context = _contextFactory.ForFault(commandEndpoint, exception);
            Event.Transition message = _eventFactory.ForStateTransition(context);

            _eventAggregator.Publish(message);
        }
    }
}
