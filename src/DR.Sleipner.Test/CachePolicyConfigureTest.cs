using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.Test.TestModel;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CachePolicyConfigureTest
    {
        [Test]
        public void TestPolicy()
        {
            CachePolicy.DefaultIs().CacheFor(100).IgnoreExceptions();
            CachePolicy.ForAll<IAwesomeInterface>().CacheFor(600).IgnoreExceptions();
            CachePolicy.For<IAwesomeInterface>(a => a.ParameteredMethod("", 0)).CacheFor(20).IgnoreExceptions();

            var bla = CachePolicy.CachePolicies;
            var kk = "";
        }
    }
}