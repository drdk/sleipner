using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;
using NUnit.Framework;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    [TestFixture]
    public class HashAlgorithmTest
    {
        [Test]
        public void TestHashFunction()
        {
            var proxyRequest = ProxyRequest<IAwesomeInterface>.FromExpression(a => a.ParameteredMethod("sss", 1));
            var hash = proxyRequest.CreateHash();
            var bla = "";
        }
    }
}
