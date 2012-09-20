using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy
{
    public class CacheProxyException : Exception
    {
        public CacheProxyException(string message) : base(message)
        {
        }
    }
}
