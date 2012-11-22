using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
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

            var methodInfo = typeof(IAwesomeInterface).GetMethod("ParameterlessMethod");
            var cachePolicy = new MethodCachePolicy()
            {
                CacheDuration = 10
            };

            var parameters = new object[] {"", 1};
            var hashKey = CacheProviderBase<IAwesomeInterface>.GenerateStringKey(methodInfo, parameters);

            var objectToStore = new[] {"", ""}.AsEnumerable();
            enyimProvider.StoreItem(methodInfo, cachePolicy, objectToStore, parameters);

            memcachedMock.Verify(a => a.Store(StoreMode.Set, hashKey, It.IsAny<MemcachedObject<IEnumerable<string>>>()), Times.Once()); //We can't test properly if it created the object correctly.
        }

        [Test]
        public void TestStoreException()
        {
            var memcachedMock = new Mock<IMemcachedClient>();
            var enyimProvider = new EnyimMemcachedProvider<IAwesomeInterface>(memcachedMock.Object);

            var methodInfo = typeof(IAwesomeInterface).GetMethod("ParameterlessMethod");
            var cachePolicy = new MethodCachePolicy()
            {
                CacheDuration = 10
            };

            var parameters = new object[] { "", 1 };
            var hashKey = CacheProviderBase<IAwesomeInterface>.GenerateStringKey(methodInfo, parameters);

            enyimProvider.StoreException<IEnumerable<string>>(methodInfo, cachePolicy, new Exception(), parameters);

            memcachedMock.Verify(a => a.Store(StoreMode.Set, hashKey, It.IsAny<MemcachedObject<IEnumerable<string>>>()), Times.Once()); //We can't test properly if it created the object correctly.
        }
    }
}
