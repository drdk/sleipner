using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy.Syncronizer
{
    public interface IRequestSyncronizer
    {
        Action GetWaitFunction(RequestKey key);
        void Release(RequestKey key);
    }
}
