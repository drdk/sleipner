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
        CachedObject<TObject> GetItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, IEnumerable<object> parameters);
        void StoreItem<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, TObject item, IEnumerable<object> parameters);
        void StoreException<TObject>(MethodInfo methodInfo, MethodCachePolicy cachePolicy, Exception exception, IEnumerable<object> parameters);

        void Purge(Expression<Action<T>> action);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();
    }
}