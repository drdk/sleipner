using System;

namespace DR.Sleipner.CacheProxy
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CacheBehaviorAttribute : Attribute
    {
        public readonly int Duration;

        /// <summary>
        /// Caches the output of a method based on it's parameters
        /// </summary>
        /// <param name="duration">Duration, in seconds, to cache the output</param>
        public CacheBehaviorAttribute(int duration)
        {
            Duration = duration;
        }
    }
}
