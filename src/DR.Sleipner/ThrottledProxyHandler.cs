using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Syncronizer;
using DR.Sleipner.Model;

namespace DR.Sleipner
{
    public class ThrottledProxyHandler<T> : IProxyHandler<T> where T : class
    {
        private readonly T _realInstance;
        private readonly ICachePolicyProvider<T> _cachePolicyProvider;
        private readonly ICacheProvider<T> _cacheProvider;

        private readonly Action<Exception> _preserveInternalException;
        private readonly IRequestSyncronizer _syncronizer = new RequestSyncronizer();

        public ThrottledProxyHandler(T realInstance, ICachePolicyProvider<T> cachePolicyProvider, ICacheProvider<T> cacheProvider)
        {
            _realInstance = realInstance;
            _cachePolicyProvider = cachePolicyProvider;
            _cacheProvider = cacheProvider;

            var preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
            if (preserveStackTrace != null)
            {
                _preserveInternalException = (Action<Exception>) Delegate.CreateDelegate(typeof (Action<Exception>), preserveStackTrace);
            }
            else
            {
                //This is to handle mono not having the InternalPreserveStackTrace method
                _preserveInternalException = e => { };
            }
        }

        public TResult HandleRequest<TResult>(string methodName, object[] parameters)
        {
            var methodInfo = typeof(T).GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            if (methodInfo.IsGenericMethod)
            {
                methodInfo = methodInfo.MakeGenericMethod(typeof (TResult).GetGenericArguments());
            }
            var cachePolicy = _cachePolicyProvider.GetPolicy(methodInfo);

            if (cachePolicy == null || cachePolicy.CacheDuration == 0)
            {
                return GetRealResult<TResult>(methodInfo, parameters);
            }

            var cachedItem = _cacheProvider.GetItem<TResult>(methodInfo, cachePolicy, parameters) ?? new CachedObject<TResult>(CachedObjectState.None, null);

            if (cachedItem.State == CachedObjectState.Fresh)
            {
                return cachedItem.Object;
            }

            if (cachedItem.State == CachedObjectState.Exception)
            {
                throw cachedItem.ThrownException;
            }

            var requestKey = new RequestKey(methodInfo, parameters);
            var waitFunction = _syncronizer.GetWaitFunction(requestKey);

            if (waitFunction != null)
            {
                if (cachedItem.State == CachedObjectState.Stale)
                    return cachedItem.Object;

                waitFunction();
                return HandleRequest<TResult>(methodName, parameters);
            }

            if (cachedItem.State == CachedObjectState.Stale)
            {
                var task = new Task<TResult>(() => GetRealResult<TResult>(methodInfo, parameters));
                task.ContinueWith(taskState =>
                {
                    try
                    {
                        if (taskState.Exception != null && cachePolicy.BubbleExceptions)
                        {
                            var exception = taskState.Exception.InnerException;
                            if (exception is TargetInvocationException)
                            {
                                _preserveInternalException(exception);
                            }

                            _cacheProvider.StoreException<TResult>(methodInfo, cachePolicy, exception, parameters);
                        }
                        else
                        {
                            var itemToStore = taskState.Exception != null ? cachedItem.Object : taskState.Result;
                            _cacheProvider.StoreItem(methodInfo, cachePolicy, itemToStore, parameters);                            
                        }
                    }
                    finally
                    {
                        _syncronizer.Release(requestKey);
                    }

                }, TaskContinuationOptions.ExecuteSynchronously);
                task.Start();
                return cachedItem.Object;
            }

            //At this point nothing is in the cache.
            TResult realInstanceResult;
            try
            {
                realInstanceResult = GetRealResult<TResult>(methodInfo, parameters);
                _cacheProvider.StoreItem(methodInfo, cachePolicy, realInstanceResult, parameters);
            }
            catch (TargetInvocationException e)
            {
                var inner = e.InnerException;
                _preserveInternalException(inner);
                _cacheProvider.StoreException<TResult>(methodInfo, cachePolicy, inner, parameters);
                throw e.InnerException;
            }
            catch (Exception e)
            {
                _cacheProvider.StoreException<TResult>(methodInfo, cachePolicy, e, parameters);
                throw;
            }
            finally
            {
                _syncronizer.Release(requestKey);
            }

            return realInstanceResult;
        }

        private TResult GetRealResult<TResult>(MethodInfo methodInfo, object[] parameters)
        {
            var delegateMethod = DelegateFactory.Create(methodInfo);
            return (TResult) delegateMethod(_realInstance, parameters);
        }
    }
}