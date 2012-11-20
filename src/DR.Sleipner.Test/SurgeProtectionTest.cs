using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public IAwesomeInterface Target;

        [SetUp]
        public void SetUp()
        {
            CallCounter = 0;

            var mock = new Mock<AwesomeImplementation>();
            mock.Setup(x => x.ParameteredMethod(It.IsAny<string>(), It.IsAny<int>())).Returns((string s, int i) =>
            {
                CallCounter++;
                Thread.Sleep(200);
                return new[] { s, "giraf" };
            });

            mock.Setup(x => x.ParameterlessMethod()).Returns(() =>
            {
                CallCounter++;
                Thread.Sleep(200);
                return new[] { "giraf" };
            });

            ICacheProvider<AwesomeImplementation> cache = new DictionaryCache<AwesomeImplementation>();
            Target = CacheProxyGenerator.GetProxy<IAwesomeInterface, AwesomeImplementation>(mock.Object, cache);
        }

        [Test]
        public void It_should_not_make_duplicate_calls_while_cache_is_in_flight()
        {
            var done = false;
            var safe = 100;

            IAsyncResult async1 = null, async2 = null;

            var action = (Action) (() => Target.ParameterlessMethod());
            var complete = (AsyncCallback)(res =>
            {
                if (async1.IsCompleted && async2.IsCompleted) { done = true; }
            });

            async1 = action.BeginInvoke(complete, null);
            Thread.Sleep(50);
            async2 = action.BeginInvoke(complete, null);

            while (!done && safe-- > 0) { Thread.Sleep(10); }

            if (safe < 0) { Assert.Fail("test did not complete in time"); }
            Assert.AreEqual(1, CallCounter, "Implementation should only be called once, was called " + CallCounter + " times.");
        }
    }
}
