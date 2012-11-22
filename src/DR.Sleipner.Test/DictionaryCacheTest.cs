using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheConfiguration;
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
            
            var bla = new CachePolicyProvider<IDummyInterface>();
            bla.For(a => a.GetProgramCards("", default(DateTime))).CacheFor(10);

            var methodInfo = typeof(IDummyInterface).GetMethod("GetProgramCards");
            var cachePolicy = bla.GetPolicy(methodInfo);

            object[] parameters = new [] {"", "1"};     
            var val = Enumerable.Empty<object>();

            cache.StoreItem(methodInfo, cachePolicy, val, parameters);
            var returnedValue = cache.GetItem<IEnumerable<object>>(methodInfo, cachePolicy, parameters);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var bla = new CachePolicyProvider<IDummyInterface>();
            bla.For(a => a.GetProgramCards("", default(DateTime))).CacheFor(2);

            var methodInfo = typeof(IDummyInterface).GetMethod("GetProgramCards");
            var cachePolicy = bla.GetPolicy(methodInfo);

            object[] parameters = new[] { "", "1" };
            var val = Enumerable.Empty<object>();

            cache.StoreItem(methodInfo, cachePolicy, val, parameters);
            Thread.Sleep(2000 + 100);
            var returnedValue = cache.GetItem<IEnumerable<object>>(methodInfo, cachePolicy, parameters);

            Assert.IsTrue(returnedValue.State == CachedObjectState.Stale);
        }
    }

    public interface IDummyInterface
    {
        void Method();

        IEnumerable<object> GetProgramCards(string bundleName, DateTime since);
    }
}
