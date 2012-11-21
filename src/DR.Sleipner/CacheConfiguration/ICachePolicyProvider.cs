using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public interface ICachePolicyProvider<T> where T : class
    {
        MethodCachePolicy GetPolicy(MethodInfo methodInfo);
        void SetPolicy(MethodInfo methodInfo, MethodCachePolicy cachePolicy);
    }
}
