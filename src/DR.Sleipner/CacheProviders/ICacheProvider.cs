using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        object GetItem(string methodName, int maxAge, params object[] parameters);
        void StoreItem(string methodName, int maxAge, object item, params object[] parameters);
        void Purge(Expression<Action<T>> action);
        bool HasItem(Expression<Action<T>> action);
        void Exterminatus();
    }
}