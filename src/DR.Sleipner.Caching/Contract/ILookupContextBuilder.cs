using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.Caching.Contract
{
    public interface ILookupContextBuilder<T> where T : class
    {
        LookupContext<T, TResult> CreateLookupContext<TResult>(T realInstance, ICacheProvider<T> cacheProvider, IPolicyProvider<T> policyProvider, MethodInfo methodInfo, object[] parameters);
    }
}
