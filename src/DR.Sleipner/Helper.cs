using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProviders;

namespace DR.Sleipner
{
    public static class Helper
    {
        public static T CreateProxy<T>(this T original, ICacheProvider<T> cacheProvider, Action<ICachePolicyProvider<T>> configExpression) where T : class
        {
            var proxy = new SleipnerProxy<T>(original, cacheProvider);
            proxy.Config(configExpression);

            return proxy.Object;
        }
    }
}
