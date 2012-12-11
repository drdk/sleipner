using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config
{
    public static class CachePolicyExtensions
    {
        /// <summary>
        /// Configures policy to cache the methods output for duration seconds
        /// The default is zero (disabled)
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static CachePolicy CacheFor(this CachePolicy policy, int duration)
        {
            policy.CacheDuration = duration;

            return policy;
        }

        /// <summary>
        /// Disables cache for this policy.
        /// This is the default setting.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static CachePolicy DisableCache(this CachePolicy policy)
        {
            policy.CacheDuration = 0;

            return policy;
        }

        /// <summary>
        /// If the real instance throws and exception, and there is a stale object already available, enabling this will supress the exception and renew the item in cache.
        /// This is the default behavior
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static CachePolicy SupressExceptionsWhenStale(this CachePolicy policy)
        {
            policy.BubbleExceptions = false;
            return policy;
        }

        /// <summary>
        /// Bubbles exceptions from real instance into cache and to the caller.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static CachePolicy BubbleExceptionsWhenStale(this CachePolicy policy)
        {
            policy.BubbleExceptions = true;
            return policy;
        }
    }
}
