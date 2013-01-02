using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.Config.Expressions;
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

            var configProvider = new BasicConfigurationProvider<IDummyInterface>();
            configProvider.For(a => a.GetProgramCards(Param.IsAny<string>(), Param.IsAny<DateTime>())).CacheFor(10);

            var proxyContext = ProxyRequest<IDummyInterface>.FromExpression(a => a.GetProgramCards("", DateTime.Now));
            var val = Enumerable.Empty<object>();

            var cachePolicy = configProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);
            Assert.IsNotNull(cachePolicy, "Config provider didn't return a cache policy");
            Assert.IsTrue(cachePolicy.CacheDuration == 10, "Cache provider returned an unexpected cache policy");

            cache.StoreItem(proxyContext, cachePolicy, val);
            var returnedValue = cache.GetItem(proxyContext, cachePolicy);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsCachedItemWithinPeriod_ListType()
        {
            var cache = new DictionaryCache<IAwesomeInterface>();

            var configProvider = new BasicConfigurationProvider<IAwesomeInterface>();
            configProvider.For(a => a.ParameteredMethod(Param.IsAny<string>(), Param.IsAny<int>(), Param.IsAny<List<string>>())).CacheFor(10);

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("a", 2, new List<string>() {"a", "b"}));
            var val = Enumerable.Empty<string>();

            var cachePolicy = configProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);
            Assert.IsNotNull(cachePolicy, "Config provider didn't return a cache policy");
            Assert.IsTrue(cachePolicy.CacheDuration == 10, "Cache provider returned an unexpected cache policy");

            cache.StoreItem(proxyContext, cachePolicy, val);
            var returnedValue = cache.GetItem(proxyContext, cachePolicy);

            Assert.AreEqual(val, returnedValue.Object);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var configProvider = new BasicConfigurationProvider<IDummyInterface>();
            configProvider.For(a => a.GetProgramCards(Param.IsAny<string>(), Param.IsAny<DateTime>())).CacheFor(2).ExpireAfter(3);

            var proxyContext = ProxyRequest<IDummyInterface>.FromExpression(a => a.GetProgramCards("", DateTime.Now));
            var val = Enumerable.Empty<object>();

            var cachePolicy = configProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);
            Assert.IsNotNull(cachePolicy, "Config provider didn't return a cache policy");
            Assert.IsTrue(cachePolicy.CacheDuration == 2, "Cache provider returned an unexpected cache policy");

            cache.StoreItem(proxyContext, cachePolicy, val);
            Thread.Sleep(2000 + 100);
            var returnedValue = cache.GetItem(proxyContext, cachePolicy);

            Assert.IsTrue(returnedValue.State == CachedObjectState.Stale);
        }

        [Test]
        public void TestReturnsStaleOutsidePeriod_AbsoluteExpiery()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var configProvider = new BasicConfigurationProvider<IDummyInterface>();
            configProvider.For(a => a.GetProgramCards(Param.IsAny<string>(), Param.IsAny<DateTime>())).CacheFor(2).ExpireAfter(3);

            var proxyContext = ProxyRequest<IDummyInterface>.FromExpression(a => a.GetProgramCards("", DateTime.Now));
            var val = Enumerable.Empty<object>();

            var cachePolicy = configProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);
            Assert.IsNotNull(cachePolicy, "Config provider didn't return a cache policy");
            Assert.IsTrue(cachePolicy.CacheDuration == 2, "Cache provider returned an unexpected cache policy");

            cache.StoreItem(proxyContext, cachePolicy, val);
            Thread.Sleep(3000 + 100);
            var returnedValue = cache.GetItem(proxyContext, cachePolicy);

            Assert.AreEqual(CachedObjectState.None, returnedValue.State);
        }
    }

    public interface IDummyInterface
    {
        void Method();

        IEnumerable<object> GetProgramCards(string bundleName, DateTime since);
    }
}
