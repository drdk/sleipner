using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class SleipnerProxyTest
    {
        [Test]
        public void TestPassThrough()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            proxy.Object.VoidMethod();

            instanceMock.Verify(a => a.VoidMethod(), Times.Once());
        }

        [Test]
        public void TestDurationCache()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.ConfigureCaching(a =>
                                       {
                                           a.ForAll().CacheFor(50).SupressExceptions(true);

                                           a.For(b => b.ParameteredMethod("", 0)).CacheFor(10).SupressExceptions(false);
                                           a.For(b => b.ParameterlessMethod()).CacheFor(10);
                                       });

            var methodReturnValue = new[] {"", ""};
            instanceMock.Setup(a => a.ParameteredMethod("", 0)).Returns(methodReturnValue);

            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);

            instanceMock.Verify(a => a.ParameteredMethod("", 0), Times.Once());
        }
    }
}