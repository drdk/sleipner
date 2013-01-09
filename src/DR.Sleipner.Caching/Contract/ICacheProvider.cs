using System;
using System.Linq.Expressions;
using DR.Sleipner.Caching.Model;

namespace DR.Sleipner.Caching.Contract
{
    public interface ICacheProvider<T> where T : class
    {
        CachedObject<TResult> GetItem<TResult>(LookupContext<T, TResult> lookupContext);
        void StoreItem<TResult>(LookupContext<T, TResult> lookupContext, TResult item);
        void StoreException<TResult>(LookupContext<T, TResult> lookupContext, Exception exception);

        void Purge<TResult>(Expression<Func<T, TResult>> expression);
        CachedObjectState GetItemState(Expression<Action<T>> action);
        void Exterminatus();
    }
}