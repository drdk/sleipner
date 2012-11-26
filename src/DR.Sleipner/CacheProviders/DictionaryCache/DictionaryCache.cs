﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCache<T> : ICacheProvider<T> where T : class
    {
        private readonly IDictionary<DictionaryCacheKey, DictionaryCachedItem> _cache = new ConcurrentDictionary<DictionaryCacheKey, DictionaryCachedItem>();

        public CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];

                if(cachedObject.ThrownException != null)
                {
                    return new CachedObject<TResult>(CachedObjectState.Exception, cachedObject.ThrownException);
                }

                return new CachedObject<TResult>(cachedObject.IsExpired ? CachedObjectState.Stale : CachedObjectState.Fresh, (TResult)cachedObject.Object);
            }

            return new CachedObject<TResult>(CachedObjectState.None, null);
        }

        public void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, TResult item)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);
            _cache[cacheKey] = new DictionaryCachedItem(item, TimeSpan.FromSeconds(cachePolicy.CacheDuration));
        }

        public void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, Exception exception)
        {
            var cacheKey = new DictionaryCacheKey(proxyRequest.Method, proxyRequest.Parameters);

            _cache[cacheKey] = new DictionaryCachedItem(exception, TimeSpan.FromSeconds(cachePolicy.ExceptionCacheDuration));
        }

        public void Purge(Expression<Action<T>> action)
        {
            throw new NotImplementedException();
        }

        public CachedObjectState GetItemState(Expression<Action<T>> action)
        {
            throw new NotImplementedException();
        }

        public void Exterminatus()
        {
            _cache.Clear();
        }
    }
}