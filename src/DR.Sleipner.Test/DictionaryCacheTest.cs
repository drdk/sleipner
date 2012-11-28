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
using DR.Sleipner.Test.TestModel;
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
        public void TestReturnsCachedItemWithinPeriod_ListType()
        {
            var cache = new DictionaryCache<IAwesomeInterface>();

            var bla = new CachePolicyProvider<IAwesomeInterface>();
            bla.For(a => a.ParameteredMethod(default(string), default(int), default(List<string>))).CacheFor(10);

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("a", 2, new List<string>() {"a", "b"}));
            var cachePolicy = bla.GetPolicy(proxyContext.Method);

            var val = Enumerable.Empty<string>();

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
