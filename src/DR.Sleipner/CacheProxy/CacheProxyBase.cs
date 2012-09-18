using System;
using System.Linq;
using System.Linq.Expressions;
using DR.Sleipner.CacheProviders;

namespace DR.Sleipner.CacheProxy
{
    public abstract class CacheProxyBase<T> where T : class
    {
        public T RealInstance;
        private readonly ICacheProvider<T> _cacheProvider;

        public CacheProxyBase(T real, ICacheProvider<T> cacheProvider)
        {
            RealInstance = real;
            _cacheProvider = cacheProvider;
        }

        public object GetCachedItem(string methodName, int maxAge, object[] parameters)
        {
            var methodInfo = typeof (T).GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var delegateMethod = DelegateFactory.Create(methodInfo);

            var cachedItem = _cacheProvider.GetItem(methodName, maxAge, parameters);
            return cachedItem;
        }

        public void StoreItem(string methodName, int maxAge, object[] parameters, object item)
        {
            _cacheProvider.StoreItem(methodName, maxAge, item, parameters);
        }
    }
}