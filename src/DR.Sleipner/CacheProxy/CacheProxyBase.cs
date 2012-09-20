using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.Model;

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
            var methodInfo = RealInstance.GetType().GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var cachedItem = _cacheProvider.GetItem(methodInfo, parameters);

            if(cachedItem == null)
            {
                //If the provider returns null we're going to assume that it means it doesn't have this in cache.
                cachedItem = new CachedObject(CachedObjectState.None, null);
            }

            if(cachedItem.State == CachedObjectState.Exception)
            {
                throw cachedItem.ThrownException;
            }

            if(cachedItem.State == CachedObjectState.Fresh)
            {
                return cachedItem.Object;
            }

            var delegateMethod = DelegateFactory.Create(methodInfo);
            if(cachedItem.State == CachedObjectState.Stale)
            {
                var task = new Task<object>(() => delegateMethod(RealInstance, parameters));
                task.ContinueWith(taskState =>
                                      {
                                          if (taskState.Exception != null)
                                          {
                                              _cacheProvider.StoreItem(methodInfo, taskState.Exception.InnerException ?? new Exception("Unknown exception was thrown by aggregateException"), parameters);
                                          }
                                          else
                                          {
                                              _cacheProvider.StoreItem(methodInfo, taskState.Result, parameters);
                                          }
                                      });
                task.Start();
                return cachedItem.Object;
            }

            //At this point nothing is in the cache.
            object realInstanceResult;
            try
            {
                realInstanceResult = delegateMethod(RealInstance, parameters);
                _cacheProvider.StoreItem(methodInfo, realInstanceResult, parameters);
            }
            catch(Exception e)
            {
                _cacheProvider.StoreItem(methodInfo, e, parameters);
                throw;
            }
            
            return realInstanceResult;
        }
    }
}