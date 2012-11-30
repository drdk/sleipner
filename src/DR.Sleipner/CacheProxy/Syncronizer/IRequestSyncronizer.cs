using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy.Syncronizer
{
    public interface IRequestSyncronizer
    {
        Func<TResult> GetWaitFunction<TResult>(RequestKey key);
        void Release<TResult>(RequestKey key, TResult result);
    }
}
