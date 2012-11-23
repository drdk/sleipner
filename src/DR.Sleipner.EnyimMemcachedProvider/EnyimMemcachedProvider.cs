using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using DR.Sleipner.Model;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace DR.Sleipner.EnyimMemcachedProvider
{
    public class EnyimMemcachedProvider<TImpl> : CacheProviderBase<TImpl>, ICacheProvider<TImpl> where TImpl : class
    {
        private readonly IMemcachedClient _client;

        public EnyimMemcachedProvider(IMemcachedClient client)
        {
            _client = client;
        }

        public CachedObject<TObject> GetItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, IEnumerable<object> parameters)
        {
            var key = GenerateStringKey(methodInfo, parameters.ToArray());

            object value;
            if (_client.TryGet(key, out value))
            {
                var cachedObject = value as MemcachedObject<TObject>;
                if (cachedObject == null)
                {
                    return new CachedObject<TObject>(CachedObjectState.None, null);
                }

                if (cachedObject.IsException && cachedObject.Created.AddSeconds(cachePolicy.ExceptionCacheDuration) > DateTime.Now)
                {
                    return new CachedObject<TObject>(CachedObjectState.Exception, cachedObject.Exception);
                }
                else if (cachedObject.IsException)
                {
                    return new CachedObject<TObject>(CachedObjectState.None, null);
                }

                var fresh = cachedObject.Created.AddSeconds(cachePolicy.CacheDuration) > DateTime.Now;
                var state = fresh ? CachedObjectState.Fresh : CachedObjectState.Stale;
                return new CachedObject<TObject>(state, cachedObject.Object);
            }

            return new CachedObject<TObject>(CachedObjectState.None, null);
        }

        public void StoreItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, TObject item, IEnumerable<object> parameters)
        {
            var key = GenerateStringKey(methodInfo, parameters.ToArray());
            var cachedObject = new MemcachedObject<TObject>()
                                   {
                                       Created = DateTime.Now,
                                       Object = item
                                   };

            _client.Store(StoreMode.Set, key, cachedObject);
        }

        public void StoreException<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, Exception exception, IEnumerable<object> parameters)
        {
            var key = GenerateStringKey(methodInfo, parameters.ToArray());
            var cachedObject = new MemcachedObject<TObject>()
            {
                Created = DateTime.Now,
                IsException = true,
                Exception = exception,
            };

            _client.Store(StoreMode.Set, key, cachedObject);
        }

        public void Purge(Expression<Action<TImpl>> action)
        {
            throw new NotImplementedException();
        }

        public CachedObjectState GetItemState(Expression<Action<TImpl>> action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is not supported in the EnyimMemcached provider
        /// </summary>
        public void Exterminatus()
        {
            throw new NotImplementedException();
        }
    }
}
