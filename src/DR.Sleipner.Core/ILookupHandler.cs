using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.Core
{
    public interface ILookupHandler<T> where T : class
    {
        TResult GetResult<TResult>(MethodInfo methodInfo, object[] parameters);
    }
}