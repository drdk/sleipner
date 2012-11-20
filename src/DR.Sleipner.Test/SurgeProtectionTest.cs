using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public int CallCounter;
        public Mock<AwesomeImplementation> Mock = new Mock<AwesomeImplementation>();
        public IAwesomeInterface Target;

        [SetUp]
        public void SetUp()
        {
            CallCounter = 0;

            Mock.Setup(x => x.ParameteredMethod(It.IsAny<string>(), It.IsAny<int>())).Returns((string s, int i) =>
            {
                CallCounter++;
                Thread.Sleep(2000);
                return new[] { s, "giraf" };
            });

            Mock.Setup(x => x.ParameterlessMethod()).Returns(() =>
            {
                CallCounter++;
                Thread.Sleep(2000);
                return new[] { "giraf" };
            });

            ICacheProvider<AwesomeImplementation> cache = new DictionaryCache<AwesomeImplementation>();
            Target = CacheProxyGenerator.GetProxy<IAwesomeInterface, AwesomeImplementation>(Mock.Object, cache);
        }

        [Test]
        public void It_should_not_make_duplicate_calls_while_cache_is_in_flight()
        {
            var tasks = new[]
                            {
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameteredMethod("", 1)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => Target.ParameterlessMethod())
                            };

            Task.WaitAll(tasks);
            Mock.Verify(a => a.ParameterlessMethod(), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 1), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 2), Times.Exactly(1));
        }
    }
}
