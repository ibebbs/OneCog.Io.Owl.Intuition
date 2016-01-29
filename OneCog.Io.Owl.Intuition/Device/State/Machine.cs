using Reactive.EventAggregator;
using System;
using System.Reactive.Linq;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    public interface IMachine
    {
        void Start();

        void Stop();
    }

    internal class Machine : IMachine
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IFactory _stateFactory;
        private readonly ITransition _transition;

        private IState _state = null;
        private IDisposable _subscription;

        public Machine(IEventAggregator eventAggregator, State.IFactory stateFactory, ITransition transition)
        {
            _eventAggregator = eventAggregator;
            _stateFactory = stateFactory;
            _transition = transition;

            _subscription = _eventAggregator.GetEvent<Event.Transition>().Subscribe(TransitionState);

            TransitionState(new Event.Transition(new State.Context.Disconnected()));
        }

        private void ConstructState(Context.IContext context)
        {
            _state = _stateFactory.For(context);
        }

        private void EnterState()
        {
            if (_state != null)
            {
                Instrumentation.State.Machine.EnteringState(_state.Name);

                _state.OnEnter();

                Instrumentation.State.Machine.EnteredState(_state.Name);
            }
        }

        private void ExitState()
        {
            if (_state != null)
            {
                State.Name name = _state.Name;

                Instrumentation.State.Machine.ExitingState(name);

                _state.OnExit();
                _state = null;

                Instrumentation.State.Machine.ExitedState(name);
            }
        }

        private void TransitionState(Event.Transition message)
        {
            ExitState();

            ConstructState(message.Context);

            EnterState();
        }

        public void Start()
        {
        }

        public void Stop()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }
    }
}
