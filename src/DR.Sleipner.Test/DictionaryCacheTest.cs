using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProviders.DictionaryCache;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    class DictionaryCacheTest
    {
        [Test]
        public void TestReturnsCachedItemWithinPeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var methodName = "metodenavn";
            var maxAge = 10;
            object[] parameters = new [] {"", "1"};
            var val = new object();

            cache.StoreItem(methodName, maxAge, val, parameters);
            var returnedValue = cache.GetItem(methodName, maxAge, parameters);

            Assert.AreEqual(val, returnedValue);
        }

        [Test]
        public void TestReturnsNullOutsidePeriod()
        {
            ICacheProvider<IDummyInterface> cache = new DictionaryCache<IDummyInterface>();

            var methodName = "metodenavn";
            var maxAge = 2;
            object[] parameters = new[] { "", "1" };
            var val = new object();

            cache.StoreItem(methodName, maxAge, val, parameters);
            Thread.Sleep(maxAge*1000 + 100);
            var returnedValue = cache.GetItem(methodName, maxAge, parameters);

            Assert.AreEqual(null, returnedValue);
        }
    }

    public interface IDummyInterface
    {
        void Method();
        object GetProgramCards(string bundleName, DateTime since);
    }
}
