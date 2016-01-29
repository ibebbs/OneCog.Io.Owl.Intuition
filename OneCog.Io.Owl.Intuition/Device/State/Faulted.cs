
using Reactive.EventAggregator;

namespace OneCog.Io.Owl.Intuition.Device.State
{
    internal class Faulted : IState
    {
        private IEventAggregator _eventAggregator;
        private ITransition _transition;
        private Context.IFault _context;

        public Faulted(IEventAggregator eventAggregator, ITransition transition, Context.IFault context)
        {
            _eventAggregator = eventAggregator;
            _transition = transition;
            _context = context;
        }

        public void OnEnter()
        {
            _eventAggregator.Publish(new Device.Event.Errored(_context.Exception));

            Instrumentation.State.Machine.Faulted(_context.Exception);
        }

        public void OnExit()
        {
            // Do nothing
        }

        public Name Name
        {
            get { return Name.Faulted; }
        }
    }
}
