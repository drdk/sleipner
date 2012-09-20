using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    class DictionaryCacheTest
    {
        [Test]
        public void TestReturnsCachedItemWithinPeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();
            var methodInfo = typeof(IDummyInterface).GetMethod("GetProgramCards");

            object[] parameters = new [] {"", "1"};
            var val = Enumerable.Empty<object>();

            cache.StoreItem(methodInfo, val, parameters);
            var returnedValue = cache.GetItem<IEnumerable<object>>(methodInfo, parameters);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();
            var methodInfo = typeof(IDummyInterface).GetMethod("GetProgramCards");

            object[] parameters = new[] { "", "1" };
            var val = Enumerable.Empty<object>();

            cache.StoreItem(methodInfo, val, parameters);
            Thread.Sleep(2000 + 100);
            var returnedValue = cache.GetItem<IEnumerable<object>>(methodInfo, parameters);

            Assert.IsTrue(returnedValue.State == CachedObjectState.Stale);
        }
    }

    public interface IDummyInterface
    {
        void Method();

        [CacheBehavior(Duration = 2)]
        IEnumerable<object> GetProgramCards(string bundleName, DateTime since);
    }
}
