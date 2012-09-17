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

        public object GetCachedItem(string methodName, int maxAge, object[] parameters)
        {
            var owner = RealInstance.GetType();

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