using System;
using System.Linq;
using DR.Sleipner.CacheProviders;

namespace DR.Sleipner.CacheProxy
{
    public abstract class CacheProxyBase<T> where T : class
    {
        public T RealInstance;
        private readonly ICacheProvider _cacheProvider;

        public CacheProxyBase(T real, ICacheProvider cacheProvider)
        {
            RealInstance = real;
            _cacheProvider = cacheProvider;
        }

        public object GetCachedItem(string methodName, object[] parameters)
        {
            var owner = RealInstance.GetType();

            /* This needs to be scrutinized for performance optimization. I don't remember if reflections internally cache all this
             * but all these are expensive reflections 
             */
            var methodInfo = typeof(T).GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var cacheAttribute = methodInfo.GetCustomAttributes(typeof(CacheBehaviorAttribute), true).OfType<CacheBehaviorAttribute>().FirstOrDefault();
            if (cacheAttribute == null)
            {
                throw new UnknownCacheBehaviorException();
            }

            var cachedItem = _cacheProvider.GetItem(owner, methodName, parameters);
            return cachedItem;
        }

        public void StoreItem(string methodName, object[] parameters, object item)
        {
            var owner = RealInstance.GetType();
            _cacheProvider.StoreItem(owner, methodName, item, parameters);
        }
    }
}