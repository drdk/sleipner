using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    [TestFixture]
    public class EnyimMemcachedProviderStoreItemTests
    {
        [Test]
        public void TestStoreItem()
        {
            var memcachedMock = new Mock<IMemcachedClient>();
            var enyimProvider = new EnyimMemcachedProvider<IAwesomeInterface>(memcachedMock.Object);

            var cachePolicy = new CachePolicy()
            {
                CacheDuration = 10
            };

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("", 1));
            var hashKey = proxyContext.CreateHash();

            var objectToStore = new[] {"", ""}.AsEnumerable();
            enyimProvider.StoreItem(proxyContext, cachePolicy, objectToStore);

            memcachedMock.Verify(a => a.Store(StoreMode.Set, hashKey, It.IsAny<MemcachedObject<IEnumerable<string>>>()), Times.Once()); //We can't test properly if it created the object correctly.
        }

        [Test]
        public void TestStoreException()
        {
            var memcachedMock = new Mock<IMemcachedClient>();
            var enyimProvider = new EnyimMemcachedProvider<IAwesomeInterface>(memcachedMock.Object);

            var cachePolicy = new CachePolicy()
            {
                CacheDuration = 10
            };

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("", 1));
            var hashKey = proxyContext.CreateHash();

            enyimProvider.StoreException<IEnumerable<string>>(proxyContext, cachePolicy, new Exception());

            memcachedMock.Verify(a => a.Store(StoreMode.Set, hashKey, It.IsAny<MemcachedObject<IEnumerable<string>>>()), Times.Once()); //We can't test properly if it created the object correctly.
        }
    }
}
