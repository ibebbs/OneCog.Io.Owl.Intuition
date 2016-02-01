using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Endpoint
{
    public static class LimitedConcurrencyScheduler
    {
        public static readonly IScheduler Single = new TaskPoolScheduler(new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1)));
    }
}
