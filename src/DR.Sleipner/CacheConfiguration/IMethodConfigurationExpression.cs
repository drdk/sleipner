using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public interface IMethodConfigurationExpression<T> where T : class
    {
        IMethodConfigurationExpression<T> CacheFor(int durationSeconds);
        IMethodConfigurationExpression<T> IgnoreExceptions();
    }
}
