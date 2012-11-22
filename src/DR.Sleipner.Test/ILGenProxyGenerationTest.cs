using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Generators;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class ILGenProxyGenerationTest
    {
        [Test]
        public void TestILGenerator()
        {
            var generator = new ILGenProxyGenerator();
            var proxyType = generator.CreateProxy<IAwesomeInterface>();

            var instanceMock = new Mock<IAwesomeInterface>();
            var proxyHandlerMock = new Mock<IProxyHandler<IAwesomeInterface>>();

            var proxy = (IAwesomeInterface) Activator.CreateInstance(proxyType, instanceMock.Object, proxyHandlerMock.Object);

            proxy.FaulyCachedMethod();
            instanceMock.Verify(a => a.FaulyCachedMethod(), Times.Never());
            proxyHandlerMock.Verify(a => a.HandleRequest<IEnumerable<string>>("FaulyCachedMethod", new object[0]), Times.Once());

            proxy.VoidMethod();
            instanceMock.Verify(a => a.VoidMethod(), Times.Once());
            proxyHandlerMock.Verify(a => a.HandleRequest<IEnumerable<string>>("VoidMethod", new object[0]), Times.Never());

            proxy.ParameteredMethod("", 1);
            instanceMock.Verify(a => a.ParameteredMethod("", 1), Times.Never());
            proxyHandlerMock.Verify(a => a.HandleRequest<IEnumerable<string>>("ParameteredMethod", new object[]{"", 1}));
        }
    }
}
