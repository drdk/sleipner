using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCache<T> : ICacheProvider<T> where T : class
    {
        private readonly IDictionary<DictionaryCacheKey, DictionaryCachedItem> _cache = new ConcurrentDictionary<DictionaryCacheKey, DictionaryCachedItem>();

        public object GetItem(string methodName, int maxAge, params object[] parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodName, parameters);
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];
                if (!cachedObject.IsExpired)
                {
                    return cachedObject.Object;
                }
            }

            return null;
        }

        public void StoreItem(string methodName, int maxAge, object item, params object[] parameters)
        {
            var cacheKey = new DictionaryCacheKey(methodName, parameters);
            _cache[cacheKey] = new DictionaryCachedItem(item, TimeSpan.FromSeconds(maxAge));
        }

        public void Purge(Expression<Action<T>> action)
        {
            throw new NotImplementedException();
        }

        public bool HasItem(Expression<Action<T>> action)
        {
            throw new NotImplementedException();
        }

        public void Exterminatus()
        {
            _cache.Clear();
        }
    }
}