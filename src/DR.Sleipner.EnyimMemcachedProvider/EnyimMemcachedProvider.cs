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
        private readonly ILogger<T> _logger;

        public EnyimMemcachedProvider(IMemcachedClient client) : this(client, new NullLogger<T>())
        {
            
        }

        public EnyimMemcachedProvider(IMemcachedClient client, ILogger<T> logger)
        {
            _client = client;
            _logger = logger;
        }

        public CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy)
        {
            var log = new LoggedTransaction("Memcached GET");
            _logger.Log(log);

            var stringRep = proxyRequest.CreateStringRepresentation(cachePolicy.CachePool);
            log.AddNote("Method: " + stringRep);

            var key = proxyRequest.CreateHash(cachePolicy.CachePool);

            log.AddNote("Key: " + key);

            object value;
            if (_client.TryGet(key, out value))
            {
                var cachedObject = value as MemcachedObject<TResult>;
                if (cachedObject == null)
                {
                    return new CachedObject<TResult>(CachedObjectState.None, null);
                }

                log.AddNote("Result was created on: " + cachedObject.Created);
                log.Ended = DateTime.Now;

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

            log.AddNote("No result in cache");
            log.Ended = DateTime.Now;

            return new CachedObject<TResult>(CachedObjectState.None, null);
        }

        public void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, TResult item)
        {
            var log = new LoggedTransaction("Memcached STORE");
            _logger.Log(log);

            var stringRep = proxyRequest.CreateStringRepresentation(cachePolicy.CachePool);
            log.AddNote("Method: " + stringRep);

            var key = proxyRequest.CreateHash(cachePolicy.CachePool);

            log.AddNote("Key: " + key);

            var cachedObject = new MemcachedObject<TResult>()
                                   {
                                       Created = DateTime.Now,
                                       Object = item
                                   };

            log.AddNote("Created is: " + cachedObject.Created);

            if(cachePolicy.MaxAge > 0)
            {
                var result = _client.Store(StoreMode.Set, key, cachedObject, TimeSpan.FromSeconds(cachePolicy.MaxAge));
                log.AddNote("Store with validFor. Result: " + result);
            }
            else
            {
                var result = _client.Store(StoreMode.Set, key, cachedObject);
                log.AddNote("Store. Result: " + result);
            }

            log.Ended = DateTime.Now;
        }

        public void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, Exception exception)
        {
            var key = proxyRequest.CreateHash(cachePolicy.CachePool);
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

        public void Purge<TResult>(Expression<Func<T, TResult>> expression, string cachePool = null)
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(expression);
            var parameters = SymbolExtensions.GetParameter(expression);
            var proxyExpression = new ProxyRequest<T, TResult>(methodInfo, parameters);

            var hash = proxyExpression.CreateHash(cachePool);
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
            throw new NotImplementedException("Exterminatus cannot be performed on a memcached based provider (it would exterminate the entire memcached cluster)");
        }

        public bool TryGetRaw<TResult>(ProxyRequest<T, TResult> proxyRequest, out object result, string cachePool)
        {
            var key = proxyRequest.CreateHash(cachePool);
            object value;
            if (_client.TryGet(key, out value))
            {
                var cachedObject = value as MemcachedObject<TResult>;
                if (cachedObject != null)
                {
                    result = cachedObject;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}