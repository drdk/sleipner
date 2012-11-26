using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DR.Sleipner.Helpers;

namespace DR.Sleipner.CacheConfiguration
{
    public static class CachePolicyExtensions
    {
        public static IMethodConfigurationExpression<T> For<T>(this ICachePolicyProvider<T> provider, Expression<Action<T>> method) where T : class
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(method);
            var cachePolicy = new MethodCachePolicy();

            provider.SetPolicy(methodInfo, cachePolicy);

            var configExpression = new MethodCachePolicyExpression<T>(cachePolicy);
            return configExpression;
        }

        public static IMethodConfigurationExpression<T> ForAll<T>(this ICachePolicyProvider<T> provider) where T : class
        {
            var defaultPolicy = new MethodCachePolicy();
            provider.SetDefaultPolicy(defaultPolicy);

            return new MethodCachePolicyExpression<T>(defaultPolicy);
        }
    }
}
