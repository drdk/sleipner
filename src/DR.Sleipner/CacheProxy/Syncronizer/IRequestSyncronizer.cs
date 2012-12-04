using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy.Syncronizer
{
    public interface IRequestSyncronizer
    {
        bool ShouldWaitForHandle<TResult>(RequestKey key, out RequestWaitHandle<TResult> waitHandle);
        void Release<TResult>(RequestKey key, TResult result);
        void ReleaseWithException<TResult>(RequestKey key, Exception thrownException);
    }
}
