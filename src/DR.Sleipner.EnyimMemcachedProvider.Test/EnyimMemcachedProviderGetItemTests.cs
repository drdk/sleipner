using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using DR.Sleipner.Model;
using Enyim.Caching;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    [TestFixture]
    public class EnyimMemcachedProviderGetItemTests
    {
        [Test]
        public void TestGetItemFromEmpty()
        {
            var memcachedMock = new Mock<IMemcachedClient>();
            var enyimProvider = new EnyimMemcachedProvider<IAwesomeInterface>(memcachedMock.Object);

            var methodInfo = typeof (IAwesomeInterface).GetMethod("ParameterlessMethod");
            var cachePolicy = new MethodCachePolicy()
                                  {
                                      CacheDuration = 10
                                  };

            var hashKey = CacheProviderBase<IAwesomeInterface>.GenerateStringKey(methodInfo, new object[0]);
            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, new object[0]);

            Assert.IsTrue(result.State == CachedObjectState.None, "Provider did not return an object with state none");
            Assert.IsNull(result.Object, "Provider did not return on object field");
            Assert.IsNull(result.ThrownException, "Provider did not return null in exception field");

            object val;
            memcachedMock.Verify(a => a.TryGet(hashKey, out val));
        }

        [Test]
        public void TestGetItemFresh()
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
            var obj = new[] {"", ""};

            object val = new MemcachedObject<IEnumerable<string>>()
                             {
                                 Created = DateTime.Now.AddSeconds(5),
                                 Object = obj
                             };

            memcachedMock.Setup(a => a.TryGet(hashKey, out val)).Returns(true);

            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, parameters);
            Assert.IsTrue(result.State == CachedObjectState.Fresh);
            Assert.AreEqual(obj, result.Object, "Provider did not return object");
            Assert.IsNull(result.ThrownException, "Provider did not return null in exception field");
        }

        [Test]
        public void TestGetItemStale()
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
            var obj = new[] { "", "" };

            object val = new MemcachedObject<IEnumerable<string>>()
            {
                Created = DateTime.Now.AddSeconds(-15),
                Object = obj
            };

            memcachedMock.Setup(a => a.TryGet(hashKey, out val)).Returns(true);

            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, parameters);
            Assert.IsTrue(result.State == CachedObjectState.Stale, "Provider did not return stale, but: " + result.State);
            Assert.AreEqual(obj, result.Object, "Provider did not return object");
            Assert.IsNull(result.ThrownException, "Provider did not return null in exception field");
        }

        [Test]
        public void TestGetItemException()
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
            var exception = new Exception("Rofl exception");

            object val = new MemcachedObject<IEnumerable<string>>()
            {
                Created = DateTime.Now.AddSeconds(5),
                IsException = true,
                Exception = exception
            };

            memcachedMock.Setup(a => a.TryGet(hashKey, out val)).Returns(true);

            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, parameters);
            Assert.IsTrue(result.State == CachedObjectState.Exception);
            Assert.AreEqual(exception, result.ThrownException, "Provider did not return stored exception");
            Assert.IsNull(result.Object, "Provider did not return null in object field");
        }

        [Test]
        public void TestGetItemExceptionExpired()
        {
            var memcachedMock = new Mock<IMemcachedClient>();
            var enyimProvider = new EnyimMemcachedProvider<IAwesomeInterface>(memcachedMock.Object);

            var methodInfo = typeof(IAwesomeInterface).GetMethod("ParameterlessMethod");
            var cachePolicy = new MethodCachePolicy()
            {
                CacheDuration = 2
            };

            var parameters = new object[] { "", 1 };

            var hashKey = CacheProviderBase<IAwesomeInterface>.GenerateStringKey(methodInfo, parameters);
            var exception = new Exception("Rofl exception");

            object val = new MemcachedObject<IEnumerable<string>>()
            {
                Created = DateTime.Now.AddSeconds(-11),
                IsException = true,
                Exception = exception
            };

            memcachedMock.Setup(a => a.TryGet(hashKey, out val)).Returns(true);

            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, parameters);
            Assert.IsTrue(result.State == CachedObjectState.None);
            Assert.IsNull(result.ThrownException, "Provider did not return null in exception field");
            Assert.IsNull(result.Object, "Provider did not return null in object field");
        }

        [Test]
        public void TestGetItemIncorrectType()
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
            var obj = new[] { "", "" };

            object val = new MemcachedObject<string>()
            {
                Created = DateTime.Now.AddSeconds(5),
                Object = ""
            };

            memcachedMock.Setup(a => a.TryGet(hashKey, out val)).Returns(true);

            var result = enyimProvider.GetItem<IEnumerable<string>>(methodInfo, cachePolicy, parameters);
            Assert.IsTrue(result.State == CachedObjectState.None);
            Assert.IsNull(result.Object, "Provider did not return null in object field");
            Assert.IsNull(result.ThrownException, "Provider did not return null in exception field");
        }
    }
}
