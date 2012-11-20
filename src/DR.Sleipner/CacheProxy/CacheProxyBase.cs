using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy.Syncronizer;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProxy
{
    public abstract class CacheProxyBase<T, TImpl> where T : class where TImpl : class, T
    {
        public TImpl RealInstance;
        private readonly ICacheProvider<TImpl> _cacheProvider;
        private readonly Action<Exception> _preserveInternalException;
        private IRequestSyncronizer _syncronizer = new RequestSyncronizer();

        public CacheProxyBase(TImpl real, ICacheProvider<TImpl> cacheProvider)
        {
            RealInstance = real;
            _cacheProvider = cacheProvider;

            // see: http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace/1663549#1663549
            MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
            _preserveInternalException = (Action<Exception>)Delegate.CreateDelegate(typeof(Action<Exception>), preserveStackTrace);
        }

        public TResult ProxyCall<TResult>(string methodName, object[] parameters)
        {
            var methodInfo = RealInstance.GetType().GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            var cachedItem = _cacheProvider.GetItem<TResult>(methodInfo, parameters) ?? new CachedObject<TResult>(CachedObjectState.None, null);

            if (cachedItem.State == CachedObjectState.Fresh)
            {
                return cachedItem.Object;
            }

            if (cachedItem.State == CachedObjectState.Exception)
            {
                throw cachedItem.ThrownException;
            }

            var delegateMethod = DelegateFactory.Create(methodInfo);
            var requestKey = new RequestKey(methodInfo, parameters);
            var waitFunction = _syncronizer.GetWaitFunction(requestKey);

            if (waitFunction != null)
            {
                if (cachedItem.State == CachedObjectState.Stale)
                    return cachedItem.Object;

                waitFunction();
                return ProxyCall<TResult>(methodName, parameters);
            }

            if (cachedItem.State == CachedObjectState.Stale)
            {
                var task = new Task<TResult>(() => (TResult)delegateMethod(RealInstance, parameters));
                task.ContinueWith(taskState =>
                {
                    try
                    {
                        if (taskState.Exception != null)
                        {
                            _cacheProvider.StoreItem(methodInfo, taskState.Exception.InnerException, parameters);
                        }
                        else
                        {
                            _cacheProvider.StoreItem(methodInfo, taskState.Result, parameters);
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
                realInstanceResult = (TResult) delegateMethod(RealInstance, parameters);
                _cacheProvider.StoreItem(methodInfo, realInstanceResult, parameters);
            }
            catch (TargetInvocationException e)
            {
                var inner = e.InnerException;
                _preserveInternalException(inner);
                _cacheProvider.StoreException<TResult>(methodInfo, inner, parameters);
                throw e.InnerException;
            }
            catch (Exception e)
            {
                _cacheProvider.StoreException<TResult>(methodInfo, e, parameters);
                throw;
            }
            finally
            {
                _syncronizer.Release(requestKey);
            }

            return realInstanceResult;
        }
    }
}