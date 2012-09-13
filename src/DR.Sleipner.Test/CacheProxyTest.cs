using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CacheProxyTest
    {
        [Test]
        public void Test()
        {
            var lol = new LolImpl();
            var cachedLol = CacheProxy.GetProxy<ILol>(lol);
            
            var b = cachedLol.Lol();
            var kk = "";
        }
    }

    public interface ILol
    {
        string Lol();
        void LolVoid(string k, int l);
    }

    public class LolImpl : ILol
    {
        public string Lol()
        {
            return "";
        }

        public void LolVoid(string k, int c)
        {
        }
    }
}
