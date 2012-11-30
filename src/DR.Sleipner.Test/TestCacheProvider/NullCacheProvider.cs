using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;

namespace DR.Sleipner.Test.TestCacheProvider
{
    public class NullCacheProvider<T> : ICacheProvider<T> where T : class
    {
        public CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy)
        {
            return null;
        }

        public void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, TResult item)
        {
        }

        public void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, Exception exception)
        {
        }

        public void Purge(Expression<Action<T>> action)
        {
        }

        public CachedObjectState GetItemState(Expression<Action<T>> action)
        {
            return CachedObjectState.None;
        }

        public void Exterminatus()
        {
        }
    }
}
