using System;

namespace DR.Sleipner.CacheProxy
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class CacheBehaviorAttribute : Attribute
    {
        /// <summary>
        /// Number of seconds the method output should be cached for
        /// </summary>
        public int Duration;

        public bool Uncached;
    }
}