using System;

namespace DR.Sleipner.CacheProxy
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CacheBehaviorAttribute : Attribute
    {
        /// <summary>
        /// Number of seconds the method output should be cached for
        /// </summary>
        public int Duration;
    }
}