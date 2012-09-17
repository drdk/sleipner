using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCache<T> : ICacheProvider<T> where T : class
    {
        private readonly IDictionary<CacheKey, CachedObject> _cache = new ConcurrentDictionary<CacheKey, CachedObject>();

        public object GetItem(string methodName, int maxAge, params object[] parameters)
        {
            var cacheKey = new CacheKey(methodName, parameters);
            if(_cache.ContainsKey(cacheKey))
            {
                var cachedObject = _cache[cacheKey];
                if (!cachedObject.IsExpired)
                {
                    return cachedObject.Object;
                }
                else
                {
                    var rofl = "";
                }
            }

            return null;
        }

        public void StoreItem(string methodName, int maxAge, object item, params object[] parameters)
        {
            var cacheKey = new CacheKey(methodName, parameters);
            _cache[cacheKey] = new CachedObject(item, TimeSpan.FromSeconds(maxAge));
        }
    }
}