using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.Config.Expressions;
using DR.Sleipner.Model;
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

            var proxy = instanceMock.Object.CreateProxy(cacheProviderMock.Object, a =>  {
                                                                                            a.DefaultIs().DisableCache();
                                                                                            a.For(b => b.NonCachedMethod()).ExpireAfter(1000);
                                                                                        });

            proxy.VoidMethod();

            instanceMock.Verify(a => a.VoidMethod(), Times.Once());
        }

        [Test]
        public void TestDurationCache()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
                             {
                                 a.DefaultIs().CacheFor(50);
                             });

            var methodReturnValue = new[] {"", ""};
            instanceMock.Setup(a => a.ParameteredMethod("", 0)).Returns(methodReturnValue);

            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);

            instanceMock.Verify(a => a.ParameteredMethod("", 0), Times.Once());
        }

        [Test]
        public void TestGenericMethod()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
            {
                a.DefaultIs().CacheFor(50);
            });

            var methodReturnValue = new[] { "", "" }.ToList();
            instanceMock.Setup(a => a.GenericMethod<string>("", 0)).Returns(methodReturnValue);
            instanceMock.Setup(a => a.GenericMethod<object>("", 0)).Returns(new object[] { 1, 2 });

            proxy.Object.GenericMethod<string>("", 0);
            proxy.Object.GenericMethod<object>("", 0);

            instanceMock.Verify(a => a.GenericMethod<string>("", 0), Times.Once());
        }

        [Test]
        public void TestStrangeGenericMethod()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
            {
                a.DefaultIs().CacheFor(50);
            });

            var dictReturn = new Dictionary<string, int>()
                                 {
                                     {"a", 1},
                                     {"b", 2},
                                     {"c", 3},
                                 };
            instanceMock.Setup(a => a.StrangeGenericMethod("a", new[] {1, 2, 3})).Returns(dictReturn);

            for(var i = 0; i < 10; i++)
            {
                var result = proxy.Object.StrangeGenericMethod("a", new[] { 1, 2, 3 });
                Assert.AreSame(result, dictReturn);
            }

            instanceMock.Verify(a => a.StrangeGenericMethod("a", new[] { 1, 2, 3 }), Times.Once());
        }

        [Test]
        public void TestStrangeGenericMethod_NoCache()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
            {
                a.DefaultIs().DisableCache();
            });

            var dictReturn = new Dictionary<string, int>()
                                 {
                                     {"a", 1},
                                     {"b", 2},
                                     {"c", 3},
                                 };
            instanceMock.Setup(a => a.StrangeGenericMethod("a", new[] { 1, 2, 3 })).Returns(dictReturn);

            for (var i = 0; i < 10; i++)
            {
                var result = proxy.Object.StrangeGenericMethod("a", new[] { 1, 2, 3 });
                Assert.AreSame(result, dictReturn);
            }

            instanceMock.Verify(a => a.StrangeGenericMethod("a", new[] { 1, 2, 3 }), Times.Exactly(10));
        }

        [Test]
        public void TestComplexParametersMethod()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
            {
                a.DefaultIs().CacheFor(50);
            });

            var methodReturnValue = new[] { "", "" }.ToList();
            instanceMock.Setup(a => a.ParameteredMethod("a", 0, new [] { "a", "b" })).Returns(methodReturnValue);
            instanceMock.Setup(a => a.ParameteredMethod("a", 0, new [] { "a", "c" })).Returns(methodReturnValue);

            for (var i = 0; i <= 10; i++)
            {
                proxy.Object.ParameteredMethod("a", 0, new[] { "a", "b" });
                proxy.Object.ParameteredMethod("a", 0, new[] { "a", "c" });
            }

            instanceMock.Verify(a => a.ParameteredMethod("a", 0, new[] { "a", "b" }), Times.Once());
            instanceMock.Verify(a => a.ParameteredMethod("a", 0, new[] { "a", "c" }), Times.Once());
        }

        [Test]
        public void TestNoCache()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();

            var proxy = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProvider);
            proxy.Config(a =>
            {
                a.DefaultIs().CacheFor(50);
                a.For(b => b.ParameteredMethod(Param.IsAny<string>(), Param.IsAny<int>())).DisableCache();
            });

            var methodReturnValue = new[] { "", "" };
            instanceMock.Setup(a => a.ParameteredMethod("", 0)).Returns(methodReturnValue);
            instanceMock.Setup(a => a.ParameterlessMethod()).Returns(methodReturnValue);

            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);
            proxy.Object.ParameteredMethod("", 0);

            proxy.Object.ParameterlessMethod();
            proxy.Object.ParameterlessMethod();
            proxy.Object.ParameterlessMethod();

            instanceMock.Verify(a => a.ParameteredMethod("", 0), Times.Exactly(3));
            instanceMock.Verify(a => a.ParameterlessMethod(), Times.Once());
        }

        [Test]
        public void TestExceptionSupression()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Config(a =>
                                   {
                                       a.For(b => b.ParameterlessMethod()).CacheFor(10);
                                   });

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameterlessMethod());
            var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);

            IEnumerable<string> result = new[] { "", "" };
            var exception = new Exception();

            cacheProviderMock.Setup(a => a.GetItem(proxyContext, cachePolicy)).Returns(new CachedObject<IEnumerable<string>>(CachedObjectState.Stale, result));
            instanceMock.Setup(a => a.ParameterlessMethod()).Throws(exception);

            sleipner.Object.ParameterlessMethod();

            Thread.Sleep(1000);

            instanceMock.Verify(a => a.ParameterlessMethod(), Times.Once());
            cacheProviderMock.Verify(a => a.GetItem(proxyContext, cachePolicy), Times.Once());
            cacheProviderMock.Verify(a => a.StoreItem(proxyContext, cachePolicy, result), Times.Once());
            cacheProviderMock.Verify(a => a.StoreException(proxyContext, cachePolicy, exception), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(AwesomeException))]
        public void TestExceptions()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();
            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Config(a =>
            {
                a.DefaultIs().CacheFor(10);
            });

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameterlessMethod());
            var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);
            var result = default(IEnumerable<string>);
            var exception = new AwesomeException();

            cacheProviderMock.Setup(a => a.GetItem(proxyContext, cachePolicy)).Returns(new CachedObject<IEnumerable<string>>(CachedObjectState.None, result));
            instanceMock.Setup(a => a.ParameterlessMethod()).Throws(exception);

            sleipner.Object.ParameterlessMethod();

            Thread.Sleep(1000);

            instanceMock.Verify(a => a.ParameterlessMethod(), Times.Once());
            cacheProviderMock.Verify(a => a.GetItem(proxyContext, cachePolicy), Times.Once());
            cacheProviderMock.Verify(a => a.StoreItem(proxyContext, cachePolicy, result), Times.Never());
            cacheProviderMock.Verify(a => a.StoreException(proxyContext, cachePolicy, exception), Times.Once());
        }

        [Test]
        public void TestExceptionBubble()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();
            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Config(a =>
            {
                a.For(b => b.ParameterlessMethod()).CacheFor(10).BubbleExceptionsWhenStale();
            });

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameterlessMethod());
            var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(proxyContext.Method, proxyContext.Parameters);

            var parameters = new object[0];
            IEnumerable<string> result = new[] { "", "" };
            var exception = new AwesomeException();

            cacheProviderMock.Setup(a => a.GetItem(proxyContext, cachePolicy)).Returns(new CachedObject<IEnumerable<string>>(CachedObjectState.Stale, result));
            instanceMock.Setup(a => a.ParameterlessMethod()).Throws(exception);

            sleipner.Object.ParameterlessMethod();

            Thread.Sleep(1000);

            instanceMock.Verify(a => a.ParameterlessMethod(), Times.Once());
            cacheProviderMock.Verify(a => a.GetItem(proxyContext, cachePolicy), Times.Once());
            cacheProviderMock.Verify(a => a.StoreItem(proxyContext, cachePolicy, result), Times.Never());
            cacheProviderMock.Verify(a => a.StoreException(proxyContext, cachePolicy, exception), Times.Once());
        }

        /*[Test]
        public void TestThrottleProxyHandler()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var returnObject = new []{"", "1"};
            instanceMock.Setup(x => x.ParameteredMethod(It.IsAny<string>(), It.IsAny<int>())).Returns((string a, int bb) => returnObject);
            var policyProvider = new BasicConfigurationProvider<IAwesomeInterface>();
            policyProvider.DefaultIs().CacheFor(60);
            var cacheProvider = new Mock<ICacheProvider<IAwesomeInterface>>(MockBehavior.Loose); 

            var handler = new ThrottledProxyHandler<IAwesomeInterface>(instanceMock.Object, policyProvider, cacheProvider.Object);

            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("", 1));

            var tasks = new List<Task<IEnumerable<string>>>();
            foreach (var i in Enumerable.Range(0, 10000))
            {
                tasks.Add(Task<IEnumerable<string>>.Factory.StartNew(() => handler.HandleRequest(proxyContext)));
            }
            
            Assert.DoesNotThrow(() => Task.WaitAll(tasks.ToArray()));
            Assert.IsTrue(tasks.All(a => a.Result == returnObject));
        }*/
    }
}
