using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProviders;
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
            const string first = "a";
            const int second = 1;
            var third = new object();

            var testObject = new
                                 {
                                     rofl = "mao"
                                 };

            var lolMock = new Mock<ITestInterface>();
            lolMock.Setup(a => a.AwesomeMethod(first, second, third)).Returns(testObject);

            var cacheProviderMock = new Mock<ICacheProvider>();

            var lol = lolMock.Object;
            var cacheProvider = cacheProviderMock.Object;

            var cachedLol = CacheProxyGenerator.GetProxy(lol, cacheProvider);
            var result = cachedLol.AwesomeMethod(first, second, third);

            //Verify that the proxy calls the method on the real implementation
            lolMock.Verify(a => a.AwesomeMethod(first, second, third));

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreEqual(result, testObject);
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

            var lolMock = new Mock<ITestInterface>();
            var cacheProviderMock = new Mock<ICacheProvider>();
            
            var lol = lolMock.Object;
            var cacheProvider = cacheProviderMock.Object;

            //Ok so here we assume how tge internal implementation is made... but I think we'll end up doing needless overabstraction to avoid it. This is not optimal but oh well.
            cacheProviderMock.Setup(a => a.GetItem(lol.GetType(), "AwesomeMethod", first, second, third)).Returns(testObject);

            var cachedLol = CacheProxyGenerator.GetProxy(lol, cacheProvider);
            var result = cachedLol.AwesomeMethod(first, second, third);

            //Verify that the proxy calls the method on the real implementation
            lolMock.Verify(a => a.AwesomeMethod(first, second, third), Times.Never());

            //Verify that the cache returns the object returned by the real implementation
            Assert.AreEqual(result, testObject);
        }
    }

    public interface ITestInterface
    {
        [CacheBehavior(Duration = 60)]
        object AwesomeMethod(string first, int second, object third);
    }
}
