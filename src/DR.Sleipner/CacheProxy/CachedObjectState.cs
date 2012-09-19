using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy
{
    public enum CachedObjectState
    {
        Fresh,
        Stale,
        Exception,
        None
    }
}
