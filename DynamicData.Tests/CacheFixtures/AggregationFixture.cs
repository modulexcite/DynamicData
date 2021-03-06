using System;
using System.Reactive.Linq;
using DynamicData.Tests.Domain;
using NUnit.Framework;
using  DynamicData.Kernel;

namespace DynamicData.Tests.CacheFixtures
{
    [TestFixture]
    public class AggregationFixture
    {
        private SourceCache<Person, string> _source;
        private IObservable<int> _accumulator;
		/// <summary>
		/// Initialises this instance.
		/// </summary>
		[SetUp]
        public void Initialise()
        {
            _source = new SourceCache<Person, string>(p => p.Name);

            _accumulator = _source.Connect().ForAggregation()
                .Scan(0, (current, items) =>
                {
                    items.ForEach(x =>
                    {
                        if (x.Type == AggregateType.Add)
                            current = current + x.Item.Age;
                        else
                            current = current - x.Item.Age;
                    });
                    return current;
                });
        }

        [TearDown]
        public void Cleanup()
        {
            _source.Dispose();
        }

        [Test]
        public void CanAccumulate()
        {
            int latest=0;
            int counter = 0;

            var accumulator = _accumulator.Subscribe(value =>
                {
                    latest = value;
                    counter++;
                });

            _source.AddOrUpdate(new Person("A",10));
            _source.AddOrUpdate(new Person("B", 20));
            _source.AddOrUpdate(new Person("C", 30));


            Assert.AreEqual(3,counter,"Should be 3 updates");
            Assert.AreEqual(60, latest, "Accumulated value should be 60");
            _source.AddOrUpdate(new Person("A", 5));

            accumulator.Dispose();
        }

        [Test]
        public void CanHandleUpdatedItem()
        {
            int latest = 0;
            int counter = 0;

            var accumulator = _accumulator.Subscribe(value =>
            {
                latest = value;
                counter++;
            });

            _source.AddOrUpdate(new Person("A", 10));
            _source.AddOrUpdate(new Person("A", 15));


            Assert.AreEqual(2, counter, "Should be 2 updates");
            Assert.AreEqual(15, latest, "Accumulated value should be 60");
            accumulator.Dispose();
        }


    }
}