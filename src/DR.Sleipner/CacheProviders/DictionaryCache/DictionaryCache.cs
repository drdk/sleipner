using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCache<T> : CacheProviderBase<T>, ICacheProvider<T> where T : class
    {
        private readonly IDictionary<DictionaryCacheKey, DictionaryCachedItem> _cache = new ConcurrentDictionary<DictionaryCacheKey, DictionaryCachedItem>();

        public CachedObject GetItem(MethodInfo methodInfo, params object[] parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodInfo.Name, parameters);
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];

                if(cachedObject.ThrownException != null)
                {
                    return new CachedObject(CachedObjectState.Exception, cachedObject.ThrownException);
                }

                return new CachedObject(cachedObject.IsExpired ? CachedObjectState.Stale : CachedObjectState.Fresh, cachedObject.Object);
            }

            return new CachedObject(CachedObjectState.None, null);
        }

        public void StoreItem(MethodInfo methodInfo, object item, params object[] parameters)
        {
            var cacheDuration = GetCacheBehavior(methodInfo);

            var cacheKey = new DictionaryCacheKey(methodInfo.Name, parameters);
            _cache[cacheKey] = new DictionaryCachedItem(item, TimeSpan.FromSeconds(cacheDuration.Duration));
        }

        public void StoreItem(MethodInfo methodInfo, Exception exception, params object[] parameters)
        {
            var cacheDuration = GetCacheBehavior(methodInfo);

            var cacheKey = new DictionaryCacheKey(methodInfo.Name, parameters);
            _cache[cacheKey] = new DictionaryCachedItem(exception, TimeSpan.FromSeconds(2));
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