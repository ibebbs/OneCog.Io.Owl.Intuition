using Reactive.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    internal class Disconnected : IState
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ITransition _transition;

        private IDisposable _subscription;

        public Disconnected(IEventAggregator eventAggregator, ITransition transition)
        {
            _eventAggregator = eventAggregator;
            _transition = transition;
        }

        public void OnEnter()
        {
            _subscription = _eventAggregator.GetEvent<Device.Event.Connect>().Subscribe(connect => _transition.ToConnecting());
        }

        public void OnExit()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        public Name Name
        {
            get { return Name.Disconnected; }
        }
    }
}
