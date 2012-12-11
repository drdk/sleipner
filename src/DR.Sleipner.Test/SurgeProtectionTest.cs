using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders.DictionaryCache;
using DR.Sleipner.Config;
using DR.Sleipner.Config.Expressions;
using DR.Sleipner.Test.TestModel;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class SurgeProtectionTest
    {
        public Mock<IAwesomeInterface> Mock = new Mock<IAwesomeInterface>();
        public IList<string> ListParam = new List<string>() {"1", "2", "3"};
            
        [SetUp]
        public void SetUp()
        {
            Mock.Setup(x => x.ParameterlessMethod()).Returns(GetStrings);
            Mock.Setup(x => x.ParameteredMethod("", 1)).Returns(GetStrings);
            Mock.Setup(x => x.ParameteredMethod("", 2)).Returns(GetStrings);

            Mock.Setup(x => x.ParameteredMethod("", 1, new[] { "1", "2", "3" })).Returns(GetStrings);
            Mock.Setup(x => x.ParameteredMethod("", 1, new[] { "d", "2", "3" })).Returns(GetStrings);
            Mock.Setup(x => x.ParameteredMethod("1", 1, new[] { "1", "2", "3" })).Returns(GetStrings);
        }

        private IEnumerable<string> GetStrings()
        {
            Thread.Sleep(2000);
            return new[] { "s", "giraf" };
        }
            
        [Test]
        public void It_should_not_make_duplicate_calls_while_cache_is_in_flight()
        {
            var cacheProvider = new DictionaryCache<IAwesomeInterface>();
            var sleipnerProxy = new SleipnerProxy<IAwesomeInterface>(Mock.Object, cacheProvider);
            sleipnerProxy.Config(a => { a.DefaultIs().CacheFor(10); });

            var target = sleipnerProxy.Object;

            var tasks = new[]
                            {
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 2)),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameterlessMethod()),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"1", "2", "3"})),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"1", "2", "3"})),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"1", "2", "3"})),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"1", "2", "3"})), //4 times
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"d", "2", "3"})),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("", 1, new[] {"d", "2", "3"})), //2 times
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("1", 1, new[] {"1", "2", "3"})),
                                Task<IEnumerable<string>>.Factory.StartNew(() => target.ParameteredMethod("1", 1, new[] {"1", "2", "3"})) //2 times
                            };

            Task.WaitAll(tasks);

            Mock.Verify(a => a.ParameterlessMethod(), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 1), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 2), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 1, new[] {"1", "2", "3"}), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("", 1, new[] {"d", "2", "3"}), Times.Exactly(1));
            Mock.Verify(a => a.ParameteredMethod("1", 1, new[] {"1", "2", "3"}), Times.Exactly(1));
        }
    }
}