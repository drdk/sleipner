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

            var proxyContext = ProxyRequest<IDummyInterface>.FromExpression(a => a.GetProgramCards("", DateTime.Now));
            var cachePolicy = bla.GetPolicy(proxyContext.Method);

            var val = Enumerable.Empty<object>();

            cache.StoreItem(proxyContext, cachePolicy, val);
            var returnedValue = cache.GetItem(proxyContext, cachePolicy);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var bla = new CachePolicyProvider<IDummyInterface>();
            bla.For(a => a.GetProgramCards("", default(DateTime))).CacheFor(2);

            var proxyContext = ProxyRequest<IDummyInterface>.FromExpression(a => a.GetProgramCards("", DateTime.Now));
            var cachePolicy = bla.GetPolicy(proxyContext.Method);

            var val = Enumerable.Empty<object>();

            cache.StoreItem(proxyContext, cachePolicy, val);
            Thread.Sleep(2000 + 100);
            var returnedValue = cache.GetItem<IEnumerable<object>>(proxyContext, cachePolicy);

            Assert.IsTrue(returnedValue.State == CachedObjectState.Stale);
        }
    }

    public interface IDummyInterface
    {
        void Method();

        IEnumerable<object> GetProgramCards(string bundleName, DateTime since);
    }
}
