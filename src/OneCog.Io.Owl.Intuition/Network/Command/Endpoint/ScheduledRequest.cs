using System;

namespace OneCog.Io.Owl.Intuition.Network.Command.Endpoint
{
    internal class ScheduledRequest
    {
        public ScheduledRequest(IRequest request, Func<IObservable<IResponse>, IObservable<IResponse>> parseResponse)
        {
            Request = request;
            ParseResponse = parseResponse;
        }

        public IRequest Request { get; private set; }

        public Func<IObservable<IResponse>, IObservable<IResponse>> ParseResponse { get; private set; }
    }
}
