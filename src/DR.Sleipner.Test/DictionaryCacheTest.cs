using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.CacheProxy;
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
            var val = new object();

            cache.StoreItem(methodInfo, val, parameters);
            var returnedValue = cache.GetItem(methodInfo, parameters);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();
            var methodInfo = typeof(IDummyInterface).GetMethod("GetProgramCards");

            object[] parameters = new[] { "", "1" };
            var val = new object();

            cache.StoreItem(methodInfo, val, parameters);
            Thread.Sleep(2000 + 100);
            var returnedValue = cache.GetItem(methodInfo, parameters);

            Assert.IsTrue(returnedValue.State == CachedObjectState.Stale);
        }
    }

    public interface IDummyInterface
    {
        void Method();

        [CacheBehavior(Duration = 2)]
        object GetProgramCards(string bundleName, DateTime since);
    }
}
