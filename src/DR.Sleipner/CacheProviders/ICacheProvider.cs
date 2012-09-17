using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProviders
{
    public interface ICacheProvider<T> where T : class
    {
        object GetItem(string methodName, int maxAge, params object[] parameters);
        void StoreItem(string methodName, int maxAge, object item, params object[] parameters);
    }
}