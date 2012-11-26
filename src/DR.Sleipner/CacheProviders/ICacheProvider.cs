using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy);
        void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, TResult item);
        void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, MethodCachePolicy cachePolicy, Exception exception);

        void Purge(Expression<Action<T>> action);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();
    }
}