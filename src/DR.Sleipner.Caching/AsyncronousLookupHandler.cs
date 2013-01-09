using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.Caching.Contract;
using DR.Sleipner.Caching.Model;

namespace DR.Sleipner.Caching
{
    public class AsyncronousLookupHandler<T> : ILookupHandler<T> where T : class
    {
        public TResult Handle<TResult>(LookupContext<T, TResult> lookupContext)
        {
            var cachedObject = lookupContext.CurrentCachedObject;
            var cacheProvider = lookupContext.CacheProvider;

            if (cachedObject.State == CachedObjectState.Fresh)
                return cachedObject.GetObject();

            if(cachedObject.State == CachedObjectState.Stale)
            {
                Func<TResult> refreshAction = () => lookupContext.GetRealInstanceResult();

                refreshAction.BeginInvoke(callback =>
                                              {
                                                  try
                                                  {
                                                      var result = refreshAction.EndInvoke(callback);
                                                      cacheProvider.StoreItem(lookupContext, result);
                                                  }
                                                  catch(Exception e)
                                                  {
                                                      cacheProvider.StoreException(lookupContext, e);
                                                  }
                                              }, null);

                return cachedObject.GetObject();
            }

            try
            {
                var result = lookupContext.GetRealInstanceResult();
                cacheProvider.StoreItem(lookupContext, result);

                return result;
            }
            catch(Exception e)
            {
                cacheProvider.StoreException(lookupContext, e);
                throw;
            }
        }
    }
}