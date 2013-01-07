using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        CachedObject<TResult> GetItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy);
        void StoreItem<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, TResult item);
        void StoreException<TResult>(ProxyRequest<T, TResult> proxyRequest, CachePolicy cachePolicy, Exception exception);

        void Purge<TResult>(Expression<Func<T, TResult>> expression);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();

        bool TryGetRaw<TResult>(ProxyRequest<T, TResult> proxyRequest, out object result);
    }
}