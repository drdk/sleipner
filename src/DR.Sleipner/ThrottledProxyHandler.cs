﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private readonly IDictionary<MethodInfo, DelegateFactory.LateBoundMethod> _lateBoundMethodCache = new Dictionary<MethodInfo, DelegateFactory.LateBoundMethod>(); 

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

        public TResult HandleRequest<TResult>(ProxyRequest<T, TResult> proxyRequest)
        {
            var cachePolicy = _cachePolicyProvider.GetPolicy(proxyRequest.Method);

            if (cachePolicy == null || cachePolicy.CacheDuration == 0)
            {
                return GetRealResult(proxyRequest);
            }

            var cachedItem = _cacheProvider.GetItem(proxyRequest, cachePolicy) ?? new CachedObject<TResult>(CachedObjectState.None, null);

            if (cachedItem.State == CachedObjectState.Fresh)
            {
                return cachedItem.Object;
            }

            if (cachedItem.State == CachedObjectState.Exception)
            {
                throw cachedItem.ThrownException;
            }

            var requestKey = new RequestKey(proxyRequest.Method, proxyRequest.Parameters);
            RequestWaitHandle<TResult> waitHandle;

            if (_syncronizer.ShouldWaitForHandle(requestKey, out waitHandle))
            {
                if (cachedItem.State == CachedObjectState.Stale)
                    return cachedItem.Object;

                return waitHandle.WaitForResult();
            }

            if (cachedItem.State == CachedObjectState.Stale)
            {
                var task = new Task<TResult>(() => GetRealResult(proxyRequest));
                task.ContinueWith(taskState =>
                {
                    Exception asyncRequestThrownException = null;
                    var asyncResult = default(TResult);
                    try
                    {
                        
                        if (taskState.Exception != null && cachePolicy.BubbleExceptions)
                        {
                            var exception = taskState.Exception.InnerException;
                            if (exception is TargetInvocationException)
                            {
                                _preserveInternalException(exception);
                            }

                            _cacheProvider.StoreException(proxyRequest, cachePolicy, exception);
                            asyncRequestThrownException = exception;
                        }
                        else
                        {
                            asyncResult = taskState.Exception != null ? cachedItem.Object : taskState.Result;
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
                            _syncronizer.Release(requestKey, asyncResult);
                        }
                    }

                }, TaskContinuationOptions.ExecuteSynchronously);
                task.Start();
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
            catch (TargetInvocationException e)
            {
                thrownException = e.InnerException;
                _preserveInternalException(thrownException);
                _cacheProvider.StoreException(proxyRequest, cachePolicy, thrownException);

                throw thrownException;
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