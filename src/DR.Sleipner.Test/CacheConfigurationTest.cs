using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CacheConfigurationTest
    {
        [Test]
        public void TestForAllConfigurationExtensions()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Configure(a =>
                                   {
                                       a.ForAll().CacheFor(10).BubbleExceptionsWhenStale(true);
                                   });

            var methods = typeof (IAwesomeInterface).GetMethods();
            foreach (var method in methods)
            {
                var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(method);
                Assert.IsNotNull(cachePolicy, "Sleipner did not create a cache policy for method: " + method.Name);
                Assert.IsTrue(cachePolicy.CacheDuration == 10, "Sleiper did not create default policy with 10s duration for method: " + method.Name);
                Assert.IsTrue(cachePolicy.BubbleExceptions, "Sleiper did not create default policy with bubble exceptions for method: " + method.Name);
            }
        }

        [Test]
        public void TestForConfigurationExtensions()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Configure(a =>
                                   {
                                       a.For(b => b.ParameterlessMethod()).CacheFor(10);
                                       a.For(b => b.ParameteredMethod("", 0)).CacheFor(100).BubbleExceptionsWhenStale(true);
                                   });

            var parameterLessMethodInfo = typeof (IAwesomeInterface).GetMethod("ParameterlessMethod");
            var parameteredMethodInfo = typeof(IAwesomeInterface).GetMethod("ParameteredMethod");
            var lolMethodInfo = typeof (IAwesomeInterface).GetMethod("LolMethod");

            var parameterLessPolicy = sleipner.CachePolicyProvider.GetPolicy(parameterLessMethodInfo);
            Assert.IsNotNull(parameterLessPolicy);
            Assert.IsTrue(parameterLessPolicy.CacheDuration == 10);
            Assert.IsFalse(parameterLessPolicy.BubbleExceptions);

            var parameteredPolicy = sleipner.CachePolicyProvider.GetPolicy(parameteredMethodInfo);
            Assert.IsNotNull(parameteredPolicy);
            Assert.IsTrue(parameteredPolicy.CacheDuration == 100);
            Assert.IsTrue(parameteredPolicy.BubbleExceptions);

            var lolMethodPolicy = sleipner.CachePolicyProvider.GetPolicy(lolMethodInfo);
            Assert.IsNull(lolMethodPolicy);
        }
    }
}