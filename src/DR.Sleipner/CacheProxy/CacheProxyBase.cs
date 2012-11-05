using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProxy
{
    public abstract class CacheProxyBase<T, TImpl> where T : class where TImpl : class, T
    {
        public TImpl RealInstance;
        private readonly ICacheProvider<TImpl> _cacheProvider;

        public CacheProxyBase(TImpl real, ICacheProvider<TImpl> cacheProvider)
        {
            RealInstance = real;
            _cacheProvider = cacheProvider;
        }

        public TResult ProxyCall<TResult>(string methodName, object[] parameters)
        {
            var methodInfo = RealInstance.GetType().GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var cachedItem = _cacheProvider.GetItem<TResult>(methodInfo, parameters);

            if(cachedItem == null)
            {
                //If the provider returns null we're going to assume that it means it doesn't have this in cache.
                cachedItem = new CachedObject<TResult>(CachedObjectState.None, null);
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
                var task = new Task<TResult>(() => (TResult)delegateMethod(RealInstance, parameters));
                task.ContinueWith(taskState =>
                                      {
                                          if (taskState.Exception != null)
                                          {
                                              _cacheProvider.StoreItem(methodInfo, taskState.Exception.InnerException, parameters);
                                          }
                                          else
                                          {
                                              _cacheProvider.StoreItem(methodInfo, taskState.Result, parameters);
                                          }
                                      }, TaskContinuationOptions.ExecuteSynchronously);
                task.Start();
                return cachedItem.Object;
            }

            //At this point nothing is in the cache.
            TResult realInstanceResult;
            try
            {
                realInstanceResult = (TResult)delegateMethod(RealInstance, parameters);
                _cacheProvider.StoreItem(methodInfo, realInstanceResult, parameters);
            }
            catch(Exception e)
            {
                _cacheProvider.StoreException<TResult>(methodInfo, e, parameters);
                throw;
            }
            
            return realInstanceResult;
        }
    }
}