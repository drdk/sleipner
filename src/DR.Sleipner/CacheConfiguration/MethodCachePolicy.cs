using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public class MethodCachePolicy
    {
        public int CacheDuration;
        public int ExceptionCacheDuration = 10;
        public bool BubbleExceptions;
    }
}
