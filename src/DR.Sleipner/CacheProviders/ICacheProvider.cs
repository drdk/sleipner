using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        CachedObject GetItem(MethodInfo method, params object[] parameters);
        void StoreItem(MethodInfo method, object item, params object[] parameters);
        void StoreItem(MethodInfo method, Exception exception, params object[] parameters);

        void Purge(Expression<Action<T>> action);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();
    }
}