using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.Config;
using DR.Sleipner.Config.Expressions;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CacheConfigurationTest
    {
        [Test]
        public void TestDefault()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Config(a =>
                                {
                                    a.DefaultIs().CacheFor(10).BubbleExceptionsWhenStale();
                                });

            var methods = typeof (IAwesomeInterface).GetMethods();
            foreach (var method in methods)
            {
                var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(method, method.GetParameters().Select(a => new object()));
                Assert.IsNotNull(cachePolicy, "Sleipner did not create a cache policy for method: " + method.Name);
                Assert.IsTrue(cachePolicy.CacheDuration == 10, "Sleiper did not create default policy with 10s duration for method: " + method.Name);
                Assert.IsTrue(cachePolicy.BubbleExceptions, "Sleiper did not create default policy with bubble exceptions for method: " + method.Name);
            }
        }

        [Test]
        public void TestUnconfigured()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);

            var methods = typeof(IAwesomeInterface).GetMethods();
            foreach (var method in methods)
            {
                var cachePolicy = sleipner.CachePolicyProvider.GetPolicy(method, method.GetParameters().Select(a => new object()));
                Assert.IsNull(cachePolicy, "Sleipner returned a policy for: " + method.Name);
            }
        }

        [Test]
        public void TestForConfigurationExtensions()
        {
            var instanceMock = new Mock<IAwesomeInterface>();
            var cacheProviderMock = new Mock<ICacheProvider<IAwesomeInterface>>();

            var sleipner = new SleipnerProxy<IAwesomeInterface>(instanceMock.Object, cacheProviderMock.Object);
            sleipner.Config(a =>
                                   {
                                       a.For(b => b.ParameterlessMethod()).CacheFor(10);
                                       a.For(b => b.ParameteredMethod(Param.IsAny<string>(), Param.IsAny<int>())).CacheFor(100).BubbleExceptionsWhenStale();
                                   });

            var parameterLessMethodInfo = typeof (IAwesomeInterface).GetMethod("ParameterlessMethod");
            var parameteredMethodInfo = typeof(IAwesomeInterface).GetMethod("ParameteredMethod", new Type[] {typeof(string), typeof(int)});
            var lolMethodInfo = typeof (IAwesomeInterface).GetMethod("LolMethod");

            var parameterLessPolicy = sleipner.CachePolicyProvider.GetPolicy(parameterLessMethodInfo, new object[0]);
            Assert.IsNotNull(parameterLessPolicy);
            Assert.IsTrue(parameterLessPolicy.CacheDuration == 10);
            Assert.IsFalse(parameterLessPolicy.BubbleExceptions);

            var parameteredPolicy = sleipner.CachePolicyProvider.GetPolicy(parameteredMethodInfo, new object[2]);
            Assert.IsNotNull(parameteredPolicy);
            Assert.IsTrue(parameteredPolicy.CacheDuration == 100);
            Assert.IsTrue(parameteredPolicy.BubbleExceptions);

            var lolMethodPolicy = sleipner.CachePolicyProvider.GetPolicy(lolMethodInfo, new object[0]);
            Assert.IsNull(lolMethodPolicy);
        }
    }
}