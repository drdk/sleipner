using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders
{
    public abstract class CacheProviderBase<T> where T : class
    {
        protected CacheBehaviorAttribute GetCacheBehavior(MethodInfo methodInfo)
        {

            var cacheAttribute = methodInfo.GetCustomAttributes(true).OfType<CacheBehaviorAttribute>();
            if(!cacheAttribute.Any())
            {
                throw new UnknownCacheBehaviorException();
            }

            return cacheAttribute.Single();
        }
    }
}
