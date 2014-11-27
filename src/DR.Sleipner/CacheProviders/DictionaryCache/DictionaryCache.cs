﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.Helpers;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCache<T> : ICacheProvider<T> where T : class
    {
        private readonly IDictionary<DictionaryCacheKey, DictionaryCachedItem> _cache = new ConcurrentDictionary<DictionaryCacheKey, DictionaryCachedItem>();

        public CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];

                if (cachedObject.ThrownException != null && cachedObject.Created.AddSeconds(cachePolicy.ExceptionCacheDuration) > DateTime.Now)
                {
                    return new CachedObject<TResult>(CachedObjectState.Exception, cachedObject.ThrownException);
                }
                else if (cachedObject.ThrownException != null)
                {
                    return new CachedObject<TResult>(CachedObjectState.None, null);
                }

                if (cachedObject.AbsoluteDuration.TotalSeconds > 0 && cachedObject.Created + cachedObject.AbsoluteDuration < DateTime.Now)
                {
                    return new CachedObject<TResult>(CachedObjectState.None, null); 
                }

                return new CachedObject<TResult>(cachedObject.IsExpired ? CachedObjectState.Stale : CachedObjectState.Fresh, (TResult)cachedObject.Object);
            }

            return new CachedObject<TResult>(CachedObjectState.None, null);
        }

        public void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, TResult item)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            var duration = TimeSpan.FromSeconds(cachePolicy.CacheDuration);
            var absoluteDuration = TimeSpan.FromSeconds(cachePolicy.MaxAge);

            _cache[cacheKey] = new DictionaryCachedItem(item, duration, absoluteDuration);
        }

        public void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, Exception exception)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            var duration = TimeSpan.FromSeconds(cachePolicy.CacheDuration);
            var absoluteDuration = TimeSpan.FromSeconds(cachePolicy.MaxAge);

            _cache[cacheKey] = new DictionaryCachedItem(exception, duration, absoluteDuration);
        }

        public void Purge<TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(expression);
            var parameters = SymbolExtensions.GetParameter(expression);
            var proxyExpression = new ProxyRequest<T, TResult>(methodInfo, parameters);

            var cacheKey = new DictionaryCacheKey(proxyExpression.Method, proxyExpression.Parameters);
            _cache.Remove(cacheKey);
        }

        public CachedObjectState GetItemState(Expression<Action<T>> action)
        {
            throw new NotImplementedException();
        }

        public void Exterminatus()
        {
            _cache.Clear();
        }

        public bool TryGetRaw<TResult>(ProxyRequest<T, TResult> proxyRequest, out object result)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            if (_cache.ContainsKey(cacheKey))
            {
                result = _cache[cacheKey];
                return true;
            }
            result = null;
            return false;
        }
    }
}