using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.Config.Expressions;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class ExpressiveConfigurationTest
    {
        [Test]
        public void TestParameterless()
        {
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameterlessMethod());
            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameterlessMethod());

            var matched = configuredMethod.IsMatch(proxyContext.Method, proxyContext.Parameters);
            Assert.IsTrue(matched, "MethodConfig didn't match");
        }

        [Test]
        public void TestParameterless_NoMatch()
        {
            var methodCachePolicy = new CachePolicy();
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameteredMethod("", 1));
            var proxyContext = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameterlessMethod());

            var matched = configuredMethod.IsMatch(proxyContext.Method, proxyContext.Parameters);
            Assert.IsFalse(matched, "MethodConfig didn't match");
        }

        [Test]
        public void TestIsAny()
        {
            var methodCachePolicy = new CachePolicy();
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameteredMethod(Param.IsAny<string>(), Param.IsAny<int>()));

            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("", 0)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod(string.Empty, 123)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("asdfasdf", -1000)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("435wasg", int.MaxValue)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("q435owiderfglæhw354t", int.MinValue)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("sdfhgert", 4654543)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("asdfzxbsergt", 593487348)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("asdfzxbsergt", -45345423)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("asdf3", 989899)));
        }

        private static bool IsMatch<T, TResult>(IConfiguredMethod<T> configuredMethod, Expression<Func<T, TResult>> expression) where T : class
        {
            var proxyContext = ProxyRequest<T>.FromExpression(expression);
            return configuredMethod.IsMatch(proxyContext.Method, proxyContext.Parameters);
        }

        [Test]
        public void TestBetweenStrings()
        {
            var methodCachePolicy = new CachePolicy();
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameteredMethod(Param.IsBetween("a", "g"), Param.IsAny<int>()));

            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("9", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("0", 1)));

            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("a", 3)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("b", 4)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("c", 2)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("dicks", 1)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("eellers", 0)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("f", 0)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("g", 0)));

            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("h", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("æabc", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("hest", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("lol", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("omfg", 0)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("trololol", 0)));
        }

        [Test]
        public void TestBetweenInts()
        {
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameteredMethod(Param.IsAny<string>(), Param.IsBetween(-1000, 1000)));
            
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod(null, -1001)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("0", 1001)));

            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("a", 1000)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("b", 900)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("c", 2)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("dicks", 1)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("eellers", 0)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("f", -900)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("g", -1000)));

            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("h", 1050)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("æabc", 10000)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("hest", int.MaxValue)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("lol", int.MinValue)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("omfg", -102032)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("trololol", 343423)));
        }

        [Test]
        public void TestBetweenDateTimeDelegates()
        {
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.DatedMethod(0, Param.IsBetween<DateTime>(() => DateTime.Now.AddHours(-2), () => DateTime.Now)));
            
            Thread.Sleep(2000);
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now.AddSeconds(-100))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now.AddSeconds(-1000))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now.AddHours(-1))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now.AddHours(-1))));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.DatedMethod(0, DateTime.Now.AddHours(-2).AddSeconds(-1))));
        }

        [Test]
        public void TestBetweenDateTimeExacts()
        {
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.DatedMethod(100, Param.IsBetween(DateTime.Now.AddHours(-2), DateTime.Now)));

            Thread.Sleep(2000);
            Assert.IsFalse(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now)));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now.AddSeconds(-100))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now.AddSeconds(-1000))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now.AddHours(-1))));
            Assert.IsTrue(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now.AddHours(-2).AddSeconds(-1))));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.DatedMethod(100, DateTime.Now.AddHours(-2).AddSeconds(-4))));
        }

        [Test]
        public void TestConstants()
        {
            var configuredMethod = GetConfiguredMethod<IAwesomeInterface>(a => a.ParameteredMethod("aaa", 200));

            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("aaa", 201)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("bbb", 200)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod("a", 1)));
            Assert.IsFalse(configuredMethod.IsMatch(a => a.ParameteredMethod(null, 0)));

            Assert.IsTrue(configuredMethod.IsMatch(a => a.ParameteredMethod("aaa", 200)));
        }

        private IConfiguredMethod<T> GetConfiguredMethod<T>(Expression<Action<T>> action) where T : class
        {
            var configuredMethod = new ExpressionConfiguredMethod<T>(action);
            return configuredMethod;
        }
    }
}