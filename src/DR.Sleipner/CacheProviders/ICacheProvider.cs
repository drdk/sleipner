using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Model;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        CachedObject<TObject> GetItem<TObject>(MethodInfo method, params object[] parameters);
        void StoreItem<TObject>(MethodInfo method, TObject item, params object[] parameters);
        void StoreException<TObject>(MethodInfo method, Exception exception, params object[] parameters);

        void Purge(Expression<Action<T>> action);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();
    }
}