using System;

namespace OneCog.Io.Owl.Intuition.Device.Event
{
    internal class Errored
    {
        public Errored(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}
