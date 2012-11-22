using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public interface IMethodConfigurationExpression<T> where T : class
    {
        /// <summary>
        /// Configures the target method to be output cached for the specified amount of seconds
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <returns></returns>
        IMethodConfigurationExpression<T> CacheFor(int durationSeconds);
        
        IMethodConfigurationExpression<T> NoCache();

        /// <summary>
        /// If enabled, will ignore exceptions thrown by the real instance, if a stale cached instance is available.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        IMethodConfigurationExpression<T> BubbleExceptionsWhenStale(bool enabled);
    }
}
