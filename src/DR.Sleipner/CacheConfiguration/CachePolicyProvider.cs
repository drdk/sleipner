using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public class CachePolicyProvider<T> : ICachePolicyProvider<T> where T : class
    {
        private readonly IDictionary<MethodInfo, MethodCachePolicy> _cachePolicies = new ConcurrentDictionary<MethodInfo, MethodCachePolicy>();

        public MethodCachePolicy GetPolicy(MethodInfo methodInfo)
        {
            return _cachePolicies.ContainsKey(methodInfo) ? _cachePolicies[methodInfo] : null;
        }

        public void SetPolicy(MethodInfo methodInfo, MethodCachePolicy cachePolicy)
        {
            _cachePolicies[methodInfo] = cachePolicy;
        }
    }
}
