using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var hashes = new List<string>()
                             {
                                 GetHash(a => a.ParameteredMethod("1", 0)),
                                 GetHash(a => a.ParameteredMethod("4", 0)),
                                 GetHash(a => a.ParameteredMethod("2", 0)),
                                 GetHash(a => a.ParameteredMethod("3", 0)),
                                 GetHash(a => a.ParameteredMethod(null, 1)),
                                 GetHash(a => a.ParameteredMethod(null, 0)),
                                 GetHash(a => a.ParameteredMethod(null, -1)),
                                 GetHash(a => a.ParameteredMethod("", 1)),
                                 GetHash(a => a.ParameteredMethod("", -1)),
                                 GetHash(a => a.ParameteredMethod("a", 0)),
                                 GetHash(a => a.ParameteredMethod("a", 1)),
                                 GetHash(a => a.ParameteredMethod("a", -1)),
                                 GetHash(a => a.ParameteredMethod("b", 0)),
                                 GetHash(a => a.ParameteredMethod(null, 11)),
                                 GetHash(a => a.ParameterlessMethod())
                             };

            Assert.IsTrue(hashes.Distinct().Count() == hashes.Count, "There was a hash collision");
        }

        private string GetHash<TResult>(Expression<Func<IAwesomeInterface, TResult>> func)
        {
            var req = ProxyRequest<IAwesomeInterface>.FromExpression(func);
            return req.CreateHash();
        }
    }
}
