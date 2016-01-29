using Reactive.EventAggregator;
using System;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    internal class Listening : IState
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ITransition _transition;
        private readonly Packet.Endpoint.IFactory _packetEndpointFactory;
        private readonly Context.IListen _context;

        private Packet.Endpoint.IInstance _packetEndpoint;
        private IDisposable _subscription;

        public Listening(IEventAggregator eventAggregator, ITransition transition, Packet.Endpoint.IFactory packetEndpointFactory, Context.IListen context)
        {
            _eventAggregator = eventAggregator;
            _transition = transition;
            _packetEndpointFactory = packetEndpointFactory;
            _context = context;
        }

        private void Publish(Packet.IReading reading)
        {
            _eventAggregator.Publish(new Device.Event.Reading(reading));
        }

        public void OnEnter()
        {
            _packetEndpoint = _packetEndpointFactory.CreateEndpoint(_context.Settings);
            _subscription = _packetEndpoint.Readings.Subscribe(Publish);

            _packetEndpoint.Open();

            _eventAggregator.Publish(new Device.Event.Started());
        }

        public void OnExit()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (_packetEndpoint != null)
            {
                _packetEndpoint.Close();
                _packetEndpoint.Dispose();
                _packetEndpoint = null;
            }
        }

        public Name Name
        {
            get { return Name.Listening; }
        }
    }
}
