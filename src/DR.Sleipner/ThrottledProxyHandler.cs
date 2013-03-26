using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Syncronizer;
using DR.Sleipner.Config;
using DR.Sleipner.Model;

namespace DR.Sleipner
{
    public class ThrottledProxyHandler<T> : IProxyHandler<T> where T : class
    {
        private readonly T _realInstance;
        private readonly ICachePolicyProvider<T> _cachePolicyProvider;
        private readonly ICacheProvider<T> _cacheProvider;
        private readonly ILogger<T> _logger;

        private readonly IRequestSyncronizer _syncronizer = new RequestSyncronizer();

        private readonly IDictionary<MethodInfo, DelegateFactory.LateBoundMethod> _lateBoundMethodCache = new Dictionary<MethodInfo, DelegateFactory.LateBoundMethod>(); 

        public ThrottledProxyHandler(T realInstance, ICachePolicyProvider<T> cachePolicyProvider, ICacheProvider<T> cacheProvider, ILogger<T> logger)
        {
            _realInstance = realInstance;
            _cachePolicyProvider = cachePolicyProvider;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        public TResult HandleRequest<TResult>(ProxyRequest<T, TResult> proxyRequest)
        {
            var cachePolicy = _cachePolicyProvider.GetPolicy(proxyRequest.Method, proxyRequest.Parameters);

            if (cachePolicy == null || cachePolicy.CacheDuration == 0)
            {
                return GetRealResult(proxyRequest);
            }

            var cachedItem = _cacheProvider.GetItem(proxyRequest, cachePolicy) ?? new CachedObject<TResult>(CachedObjectState.None, null);

            var sleiperTransactionLog = new LoggedTransaction("Started " + cachedItem.State);
            sleiperTransactionLog.AddNote("Params: " + string.Join(", ", proxyRequest.Parameters));
            _logger.Log(sleiperTransactionLog);

            if (cachedItem.State == CachedObjectState.Fresh)
            {
                sleiperTransactionLog.Ended = DateTime.Now;
                sleiperTransactionLog.AddNote("Returned item directly");

                return cachedItem.Object;
            }

            if (cachedItem.State == CachedObjectState.Exception)
            {
                sleiperTransactionLog.Ended = DateTime.Now;
                sleiperTransactionLog.AddNote("Threw cached exception to called");

                throw cachedItem.ThrownException;
            }

            var requestKey = new RequestKey(proxyRequest.Method, proxyRequest.Parameters);
            RequestWaitHandle<TResult> waitHandle;

            if (_syncronizer.ShouldWaitForHandle(requestKey, out waitHandle))
            {
                sleiperTransactionLog.AddNote("Got wait handle");
                if (cachedItem.State == CachedObjectState.Stale)
                {
                    sleiperTransactionLog.AddNote("Current item is stale, so returning object without waiting");
                    sleiperTransactionLog.Ended = DateTime.Now;

                    return cachedItem.Object;
                }

                sleiperTransactionLog.AddNote("Waiting for release from wait handle");
                var result = waitHandle.WaitForResult();

                sleiperTransactionLog.Ended = DateTime.Now;
                sleiperTransactionLog.AddNote("Done waiting");

                return result;
            }

            if (cachedItem.State == CachedObjectState.Stale)
            {
                sleiperTransactionLog.AddNote("Dispatching asyncronous handle");
                Func<TResult> loader = () => GetRealResult(proxyRequest);

                loader.BeginInvoke(callback =>
                {
                    Exception asyncRequestThrownException = null;
                    var asyncResult = default(TResult);

                    try
                    {
                        sleiperTransactionLog.AddNote("Waiting for invoke to end");
                        asyncResult = loader.EndInvoke(callback);
                        sleiperTransactionLog.AddNote("Invoke ended");
                        _cacheProvider.StoreItem(proxyRequest, cachePolicy, asyncResult);
                        sleiperTransactionLog.AddNote("Object stored");
                    }
                    catch (Exception e)
                    {
                        sleiperTransactionLog.AddNote("Invoke threw exception: " + e.Message);
                        if (cachePolicy.BubbleExceptions)
                        {
                            _cacheProvider.StoreException(proxyRequest, cachePolicy, e);
                            asyncRequestThrownException = e;
                        }
                        else
                        {
                            asyncResult = cachedItem.Object;
                            _cacheProvider.StoreItem(proxyRequest, cachePolicy, asyncResult);
                        }
                    }
                    finally
                    {
                        if (asyncRequestThrownException != null)
                        {
                            _syncronizer.ReleaseWithException<TResult>(requestKey, asyncRequestThrownException);
                        }
                        else
                        {
                            sleiperTransactionLog.AddNote("Releasing result");
                            _syncronizer.Release(requestKey, asyncResult);
                        }

                        sleiperTransactionLog.Ended = DateTime.Now;
                    }

                }, null);

                sleiperTransactionLog.AddNote("Returning object to caller");
                return cachedItem.Object;
            }

            //At this point nothing is in the cache.
            var realInstanceResult = default(TResult);
            Exception thrownException = null;
            try
            {
                realInstanceResult = GetRealResult(proxyRequest);
                _cacheProvider.StoreItem(proxyRequest, cachePolicy, realInstanceResult);
            }
            catch (Exception e)
            {
                thrownException = e;
                _cacheProvider.StoreException(proxyRequest, cachePolicy, thrownException);

                throw;
            }
            finally
            {
                if (thrownException != null)
                {
                    _syncronizer.ReleaseWithException<TResult>(requestKey, thrownException);
                }
                else
                {
                    _syncronizer.Release(requestKey, realInstanceResult);
                }
            }

            return realInstanceResult;
        }
        
        private TResult GetRealResult<TResult>(ProxyRequest<T, TResult> proxyRequest)
        {
            var delegateMethod = GetLateBoundMethod(proxyRequest.Method);

            return (TResult)delegateMethod(_realInstance, proxyRequest.Parameters);
        }

        private DelegateFactory.LateBoundMethod GetLateBoundMethod(MethodInfo methodInfo)
        {
            DelegateFactory.LateBoundMethod lateBoundMethod;
            if (!_lateBoundMethodCache.TryGetValue(methodInfo, out lateBoundMethod))
            {
                lateBoundMethod = DelegateFactory.Create(methodInfo);
                _lateBoundMethodCache[methodInfo] = lateBoundMethod;
            }

            return lateBoundMethod;
        } 
    }
}