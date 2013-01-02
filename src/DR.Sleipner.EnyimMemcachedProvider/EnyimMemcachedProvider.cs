using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using DR.Sleipner.Helpers;
using DR.Sleipner.Model;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace DR.Sleipner.EnyimMemcachedProvider
{
    public class EnyimMemcachedProvider<T> : ICacheProvider<T> where T : class
    {
        private readonly IMemcachedClient _client;

        public EnyimMemcachedProvider(IMemcachedClient client)
        {
            _client = client;
        }

        public CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy)
        {
            var key = proxyRequest.CreateHash();

            object value;
            if (_client.TryGet(key, out value))
            {
                var cachedObject = value as MemcachedObject<TResult>;
                if (cachedObject == null)
                {
                    return new CachedObject<TResult>(CachedObjectState.None, null);
                }

                if (cachedObject.IsException && cachedObject.Created.AddSeconds(cachePolicy.ExceptionCacheDuration) > DateTime.Now)
                {
                    return new CachedObject<TResult>(CachedObjectState.Exception, cachedObject.Exception);
                }
                else if (cachedObject.IsException)
                {
                    return new CachedObject<TResult>(CachedObjectState.None, null);
                }

                var fresh = cachedObject.Created.AddSeconds(cachePolicy.CacheDuration) > DateTime.Now;
                var state = fresh ? CachedObjectState.Fresh : CachedObjectState.Stale;
                return new CachedObject<TResult>(state, cachedObject.Object);
            }

            return new CachedObject<TResult>(CachedObjectState.None, null);
        }

        public void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, TResult item)
        {
            var key = proxyRequest.CreateHash();
            var cachedObject = new MemcachedObject<TResult>()
                                   {
                                       Created = DateTime.Now,
                                       Object = item
                                   };

            if(cachePolicy.MaxAge > 0)
            {
                _client.Store(StoreMode.Set, key, cachedObject, TimeSpan.FromSeconds(cachePolicy.MaxAge));
            }
            else
            {
                _client.Store(StoreMode.Set, key, cachedObject);
            }
        }

        public void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, Exception exception)
        {
            var key = proxyRequest.CreateHash();
            var cachedObject = new MemcachedObject<TResult>()
            {
                Created = DateTime.Now,
                IsException = true,
                Exception = exception,
            };

            if (cachePolicy.MaxAge > 0)
            {
                _client.Store(StoreMode.Set, key, cachedObject, TimeSpan.FromSeconds(cachePolicy.MaxAge));
            }
            else
            {
                _client.Store(StoreMode.Set, key, cachedObject);
            }
        }

        public void Purge<TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(expression);
            var parameters = SymbolExtensions.GetParameter(expression);
            var proxyExpression = new ProxyRequest<T, TResult>(methodInfo, parameters);

            var hash = proxyExpression.CreateHash();
            _client.Remove(hash);
        }

        public CachedObjectState GetItemState(Expression<Action<T>> action)
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