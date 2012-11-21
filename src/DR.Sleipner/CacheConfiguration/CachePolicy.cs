using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DR.Sleipner.Helpers;

namespace DR.Sleipner.CacheConfiguration
{
    public static class CachePolicy
    {
        public static Dictionary<MethodInfo, MethodCachePolicy> CachePolicies = new Dictionary<MethodInfo, MethodCachePolicy>();
        public static MethodCachePolicy DefaultConfiguration;

        public static MethodCachePolicyExpression<T> For<T>(Expression<Action<T>> method) where T : class
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(method);
            MethodCachePolicy cachePolicy;
            if (CachePolicies.TryGetValue(methodInfo, out cachePolicy))
            {
                cachePolicy = CachePolicies[methodInfo];
            }
            else
            {
                cachePolicy = new MethodCachePolicy();
                CachePolicies[methodInfo] = cachePolicy;
            }

            var configExpression = new MethodCachePolicyExpression<T>(cachePolicy);
            return configExpression;
        }

        public static MethodCachePolicyExpression<T> ForAll<T>() where T : class
        {
            var policies = GetPolicies(typeof (T)).ToList();

            var configurationExpression = new MethodCachePolicyExpression<T>(policies);
            return configurationExpression;
        }

        public static MethodCachePolicyExpression<object> DefaultIs()
        {
            DefaultConfiguration = new MethodCachePolicy();
            return new MethodCachePolicyExpression<object>(DefaultConfiguration);
        } 
        
        public static MethodCachePolicy GetPolicy(MethodInfo method)
        {
            MethodCachePolicy cachePolicy;
            cachePolicy = CachePolicies.TryGetValue(method, out cachePolicy) ? CachePolicies[method] : DefaultConfiguration;

            return cachePolicy;
        }

        private static IEnumerable<MethodCachePolicy> GetPolicies(Type type)
        {
            foreach (var methodInfo in type.GetMethods())
            {
                var innerMethodInfo = methodInfo;
                MethodCachePolicy cachePolicy;
                if (CachePolicies.TryGetValue(innerMethodInfo, out cachePolicy))
                {
                    cachePolicy = CachePolicies[innerMethodInfo];
                }
                else
                {
                    cachePolicy = new MethodCachePolicy();
                    CachePolicies[innerMethodInfo] = cachePolicy;
                }

                yield return cachePolicy;
            }
        }
    }
}
