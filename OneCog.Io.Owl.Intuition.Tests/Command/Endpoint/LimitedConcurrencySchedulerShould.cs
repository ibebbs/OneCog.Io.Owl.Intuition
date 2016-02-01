using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Tests.Command.Endpoint.Command.Endpoint
{
    [TestFixture]
    public class LimitedConcurrencySchedulerShould
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);


        [SetUp]
        public void TestSetUp()
        {
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [Test]
        public async Task ShouldProcessItemsSequentiallyWhenBlocking()
        {
            IList<Tuple<int, long>> result = await Observable
                .Interval(TimeSpan.FromMilliseconds(10))
                .ObserveOn(LimitedConcurrencyScheduler.Single)
                .Select(
                    (ticks, index) =>
                    {
                        Thread.Sleep(Random.Next(100));
                        return Tuple.Create(index, ticks);
                    })
                .Take(10)
                .ToList();

            IEnumerable<int> actual = result.Select(tuple => tuple.Item1).ToArray();
            IEnumerable<int> expected = Enumerable.Range(0, 10).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ShouldProcessItemsSequentiallyWhenNonBlocking()
        {
            IList<Tuple<int, long>> result = await Observable
                .Interval(TimeSpan.FromMilliseconds(10))
                .ObserveOn(LimitedConcurrencyScheduler.Single)
                .SelectMany<long, Tuple<int,long>>(
                    async (ticks, index) =>
                    {
                        Debug.WriteLine("Starting task {0}", index);
                        await Task.Delay(Random.Next(100));
                        Debug.WriteLine("Completed task {0}", index);
                        return Tuple.Create(index, ticks);
                    })
                .Take(10)
                .ToList();

            IEnumerable<int> actual = result.Select(tuple => tuple.Item1).ToArray();
            IEnumerable<int> expected = Enumerable.Range(0, 10).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
