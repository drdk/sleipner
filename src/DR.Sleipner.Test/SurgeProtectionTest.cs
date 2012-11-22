using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;
using DR.Sleipner.CacheProviders;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class SurgeProtectionTest
    {
        public Mock<IAwesomeInterface> Mock = new Mock<IAwesomeInterface>();
        
        [SetUp]
        public void SetUp()
        {
            Mock.Setup(x => x.ParameteredMethod(It.IsAny<string>(), It.IsAny<int>())).Returns((string s, int i) =>
            {
                Thread.Sleep(2000);
                return new[] { s, "giraf" };
            });

            Mock.Setup(x => x.ParameterlessMethod()).Returns(() =>
            {
                Thread.Sleep(2000);
                return new[] { "giraf" };
            });
        }

        [Test]
        public void It_should_not_make_duplicate_calls_while_cache_is_in_flight()
        {
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();
            var sleipnerProxy = new SleipnerProxy<IAwesomeInterface>(Mock.Object, cacheProvider);
            sleipnerProxy.Configure(a =>
                                               {
                                                   a.ForAll().CacheFor(10);
                                               });

            var target = sleipnerProxy.Object;

            var tasks = new[]
                            {
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod())
                            };

            Task.WaitAll(tasks);

            Mock.Verify(a => a.ParameterlessMethod(), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 1), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 2), Times.Exactly(1));
        }
    }
}