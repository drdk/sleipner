using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using DR.Sleipner.Model;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace DR.Sleipner.EnyimMemcachedProvider
{
    public class EnyimMemcachedProvider<T> : CacheProviderBase<T>, ICacheProvider<T> where T : class
    {
        private readonly IMemcachedClient _client;

        public EnyimMemcachedProvider(IMemcachedClient client)
        {
            _client = client;
        }

        public CachedObject<TObject> GetItem<TObject>(MethodInfo method, params object[] parameters)
        {
            var key = GenerateStringKey(method, parameters);
            var cacheBehavior = GetCacheBehavior(method);

            object value;
            if (_client.TryGet(key, out value))
            {
                if(!(value is MemcachedObject<TObject>))
                {
                    return new CachedObject<TObject>(CachedObjectState.None, null);
                }

                var cachedObject = (MemcachedObject<TObject>) value;
                if(cachedObject.IsException && cachedObject.Created.AddSeconds(2) < DateTime.Now)
                {
                    return new CachedObject<TObject>(CachedObjectState.Exception, new Exception("Exception stored in memcached"));
                }
                if(!cachedObject.IsException)
                {
                    var stale = cachedObject.Created.AddSeconds(cacheBehavior.Duration) < DateTime.Now;
                    return new CachedObject<TObject>(stale ? CachedObjectState.Stale : CachedObjectState.Fresh, cachedObject.Object);
                }
            }

            return new CachedObject<TObject>(CachedObjectState.None, null);
        }

        public void StoreItem<TObject>(MethodInfo method, TObject item, params object[] parameters)
        {
            var key = GenerateStringKey(method, parameters);
            var cachedObject = new MemcachedObject<TObject>()
                                   {
                                       Created = DateTime.Now,
                                       Object = item
                                   };

            _client.Store(StoreMode.Set, key, cachedObject);
        }

        public void StoreItem(MethodInfo method, Exception exception, params object[] parameters)
        {
            var key = GenerateStringKey(method, parameters);
            var cachedObject = new MemcachedObject<object>()
            {
                Created = DateTime.Now,
                IsException = true,
            };

            _client.Store(StoreMode.Set, key, cachedObject);
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
            _client.FlushAll();
        }
    }
}
