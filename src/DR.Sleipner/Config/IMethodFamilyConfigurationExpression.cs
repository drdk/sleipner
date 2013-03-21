using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config
{
    public interface IMethodFamilyConfigurationExpression
    {
        /// <summary>
        /// Configures policy to cache the methods output for duration seconds
        /// The default is zero (disabled)
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        IMethodFamilyConfigurationExpression CacheFor(int duration);

        /// <summary>
        /// The maximum age the object can have in cache before it is considered unusable. Must be larger than duration specified in CacheFor
        /// </summary>
        /// <param name="maxDuration"></param>
        /// <returns></returns>
        IMethodFamilyConfigurationExpression ExpireAfter(int maxDuration);

        /// <summary>
        /// Disables cache for this policy.
        /// This is the default setting.
        /// </summary>
        /// <returns></returns>
        void DisableCache();

        /// <summary>
        /// If the real instance throws and exception, and there is a stale object already available, enabling this will supress the exception and renew the item in cache.
        /// This is the default behavior
        /// </summary>
        /// <returns></returns>
        IMethodFamilyConfigurationExpression SupressExceptionsWhenStale();

        /// <summary>
        /// Bubbles exceptions from real instance into cache and to the caller.
        /// </summary>
        /// <returns></returns>
        IMethodFamilyConfigurationExpression BubbleExceptionsWhenStale();

        /// Cake of hest and win
        IMethodFamilyConfigurationExpression CachePool(string name);
    }
}
