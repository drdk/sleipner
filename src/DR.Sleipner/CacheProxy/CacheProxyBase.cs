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

        public object ProxyCall(string methodName, object[] parameters)
        {
            //Cache these two lines perhaps?
            var methodInfo = typeof (T).GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var delegateMethod = DelegateFactory.Create(methodInfo);

            var cachedItem = _cacheProvider.GetItem(methodName, 10, parameters);
            return cachedItem;
        }
    }
}