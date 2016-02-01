using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Command.Endpoint
{
    internal class ScheduledRequest
    {
        public ScheduledRequest(IRequest request, Func<IObservable<IResponse>, Task> parseResponse)
        {
            Request = request;
            ParseResponse = parseResponse;
        }

        public IRequest Request { get; private set; }

        public Func<IObservable<IResponse>, Task> ParseResponse { get; private set; }
    }
}
