using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.CacheProxy;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CacheProxyGeneratorTest
    {
        [Test]
        public void TestEmptyCachePassThrough()
        {
            var testObject = new
                                 {
                                     rofl = "mao"
                                 };

            var interfaceMock = new Mock<ITestInterface>();
            interfaceMock.Setup(a => a.PassThroughMethod()).Returns(testObject);
            var cacheProviderMock = new Mock<ICacheProvider<ITestInterface>>();

            var repository = interfaceMock.Object;
            var cacheProvider = cacheProviderMock.Object;
            
            var cachedLol = CacheProxyGenerator.GetProxy(repository, cacheProvider);
            var result = cachedLol.PassThroughMethod();

            //Verify that the proxy calls the method on the real implementation
            interfaceMock.Verify(a => a.PassThroughMethod());

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreEqual(result, testObject);
        }

        [Test]
        public void TestEmptyCacheVoidPassThrough()
        {
            var interfaceMock = new Mock<ITestInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<ITestInterface>>();

            var repository = interfaceMock.Object;
            var cacheProvider = cacheProviderMock.Object;

            var cachedLol = CacheProxyGenerator.GetProxy(repository, cacheProvider);
            cachedLol.VoidPassThroughMethod();

            //Verify that the proxy calls the method on the real implementation
            interfaceMock.Verify(a => a.VoidPassThroughMethod());
        }

        [Test]
        public void TestCacheIntercept()
        {
            const string first = "a";
            const int second = 1;
            var third = new object();

            var testObject = new
            {
                rofl = "mao"
            };

            var interfaceMock = new Mock<ITestInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<ITestInterface>>();
            
            var repository = interfaceMock.Object;
            var cacheProvider = cacheProviderMock.Object;
            var methodInfo = repository.GetType().GetMethod("AwesomeMethod");

            //Ok so here we assume how tge internal implementation is made... but I think we'll end up doing needless overabstraction to avoid it. This is not optimal but oh well.
            cacheProviderMock.Setup(a => a.GetItem(methodInfo, first, second, third)).Returns(new CachedObject(CachedObjectState.Fresh, testObject));

            var cachedLol = CacheProxyGenerator.GetProxy(repository, cacheProvider);
            var result = cachedLol.AwesomeMethod(first, second, third);

            //Verify that the proxy calls the method on the real implementation
            interfaceMock.Verify(a => a.AwesomeMethod(first, second, third), Times.Never());

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreSame(result, testObject);
        }

        [Test]
        public void TestCacheInterceptWithException()
        {
            const string first = "a";
            const int second = 1;
            var third = new object();

            var testObject = new
            {
                rofl = "mao"
            };

            var interfaceMock = new Mock<ITestInterface>();
            interfaceMock.Setup(a => a.AwesomeMethod(first, second, third)).Throws(new Exception());
            var repository = interfaceMock.Object;

            var cacheProvider = new DictionaryCache<ITestInterface>();
            var methodInfo = repository.GetType().GetMethod("AwesomeMethod");

            var cachedLol = CacheProxyGenerator.GetProxy(repository, cacheProvider);
            var result = cachedLol.AwesomeMethod(first, second, third);

            //Verify that the proxy calls the method on the real implementation
            interfaceMock.Verify(a => a.AwesomeMethod(first, second, third), Times.Never());

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreSame(result, testObject);
        }

        [Test]
        public void TestCacheAsyncUpdate()
        {
            const string first = "a";
            const int second = 1;
            var third = new object();

            var testObject = new
            {
                rofl = "mao"
            };

            var interfaceMock = new Mock<ITestInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<ITestInterface>>();

            var repository = interfaceMock.Object;
            var cacheProvider = cacheProviderMock.Object;
            var methodInfo = repository.GetType().GetMethod("AwesomeMethod");

            interfaceMock.Setup(a => a.AwesomeMethod(first, second, third)).Returns(testObject);
            cacheProviderMock.Setup(a => a.GetItem(methodInfo, first, second, third)).Returns(new CachedObject(CachedObjectState.Stale, testObject));

            var cachedLol = CacheProxyGenerator.GetProxy(repository, cacheProvider);
            var result = cachedLol.AwesomeMethod(first, second, third);

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreSame(result, testObject);
            
            Thread.Sleep(1000); //Yea yea, this is not nice. But the implementation is doing threading with TPL so it might have a short start delay

            //Verify that the proxy calls the method on the real implementation
            interfaceMock.Verify(a => a.AwesomeMethod(first, second, third), Times.Once());
            cacheProviderMock.Verify(a => a.StoreItem(methodInfo, testObject, first, second, third));
        }
    }

    public interface ITestInterface
    {
        object PassThroughMethod();
        void VoidPassThroughMethod();

        [CacheBehavior(Duration = 30)]
        object AwesomeMethod(string first, int second, object third);
    }
}