using FakeItEasy;
using FakeItEasy.Configuration;
using FakeItEasy.Core;
using FakeItEasy.Creation;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Endpoint
{
    public class Within
    {
        public static Within TimeSpan(TimeSpan timeSpan)
        {
            return new Within(timeSpan);
        }

        private Within(TimeSpan timeout)
        {
            TimeOut = timeout;
        }

        public TimeSpan TimeOut { get; private set; }
    }

    public class CallSynchronizer : IInterceptionListener
    {
        private static readonly object synchronizationLock = new object();

        public void OnBeforeCallIntercepted(IFakeObjectCall interceptedCall)
        {
            Monitor.Enter(synchronizationLock);
        }

        public void OnAfterCallIntercepted(ICompletedFakeObjectCall interceptedCall, IFakeObjectCallRule ruleThatWasApplied)
        {
            Monitor.Exit(synchronizationLock);
        }
    }


    public static class FakeItEasyExtensions
    {
        public static async Task MustHaveHappened<T>(this IReturnValueArgumentValidationConfiguration<T> configuration, Within within)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            CancellationTokenSource cts = new CancellationTokenSource();

            configuration.Invokes(
                call =>
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.TrySetResult(new object());
                        cts.Cancel();
                    }
                }
            );

            Task timeout = Task.Delay(within.TimeOut, cts.Token);

            Task completedTask = await Task.WhenAny(tcs.Task, timeout);

            Assert.That(completedTask, Is.EqualTo(tcs.Task), "Call to faked object timed out");
        }

        public static IFakeOptionsBuilder<T> Synchronized<T>(this IFakeOptionsBuilder<T> builder)
        {
            return builder.OnFakeCreated(fake => Fake.GetFakeManager(fake).AddInterceptionListener(new CallSynchronizer()));
        }

    }
}
