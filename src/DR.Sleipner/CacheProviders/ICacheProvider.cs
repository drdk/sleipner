using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider
    {
        object GetItem(Type owner, string methodName, params object[] parameters);
        void StoreItem(Type owner, string methodName, object item, params object[] parameters);
    }
}
