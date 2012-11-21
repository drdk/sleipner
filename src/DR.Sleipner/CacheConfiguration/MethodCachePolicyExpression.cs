using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public class MethodCachePolicyExpression<T> : IMethodConfigurationExpression<T> where T : class
    {
        private readonly IList<MethodCachePolicy> _methodCachePolicies;

        public MethodCachePolicyExpression(MethodCachePolicy methodCachePolicy) : this(new List<MethodCachePolicy>() { methodCachePolicy })
        {
        }

        public MethodCachePolicyExpression(IList<MethodCachePolicy> methodCachePolicies)
        {
            _methodCachePolicies = methodCachePolicies;
        }

        public IMethodConfigurationExpression<T> CacheFor(int durationSeconds)
        {
            SetForAll(a => a.CacheDuration = durationSeconds);
            
            return new MethodCachePolicyExpression<T>(_methodCachePolicies);
        }

        public IMethodConfigurationExpression<T> SupressExceptions(bool enabled)
        {
            SetForAll(a => a.SupressExceptions = enabled);

            return new MethodCachePolicyExpression<T>(_methodCachePolicies);
        }

        private void SetForAll(Action<MethodCachePolicy> action)
        {
            foreach (var cachePolicy in _methodCachePolicies)
            {
                action(cachePolicy);
            }
        }
    }
}