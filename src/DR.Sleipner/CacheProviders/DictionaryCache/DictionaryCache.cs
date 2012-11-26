using System;
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
    public class DictionaryCache<T> : CacheProviderBase<T>, ICacheProvider<T> where T : class
    {
        private readonly IDictionary<DictionaryCacheKey, DictionaryCachedItem> _cache = new ConcurrentDictionary<DictionaryCacheKey, DictionaryCachedItem>();

        public CachedObject<TObject> GetItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, IEnumerable<object> parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodInfo, parameters.ToArray());
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];

                if(cachedObject.ThrownException != null)
                {
                    return new CachedObject<TObject>(CachedObjectState.Exception, cachedObject.ThrownException);
                }

                return new CachedObject<TObject>(cachedObject.IsExpired ? CachedObjectState.Stale : CachedObjectState.Fresh, (TObject)cachedObject.Object);
            }

            return new CachedObject<TObject>(CachedObjectState.None, null);
        }

        public void StoreItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, TObject item, IEnumerable<object> parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodInfo, parameters.ToArray());
            _cache[cacheKey] = new DictionaryCachedItem(item, TimeSpan.FromSeconds(cachePolicy.CacheDuration));
        }

        public void StoreException<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, Exception exception, IEnumerable<object> parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodInfo, parameters.ToArray());

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