using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCachedItem
    {
        public readonly object Object;
        public readonly Exception ThrownException;
        public readonly DateTime Created;
        public readonly TimeSpan Duration;
        public readonly TimeSpan AbsoluteDuration;

        public bool IsExpired
        {
            get { return Created + Duration < DateTime.Now; }
        }

        public DictionaryCachedItem(object obj, TimeSpan duration, TimeSpan absoluteDuration)
        {
            Object = obj;
            Created = DateTime.Now;

            Duration = duration;
            AbsoluteDuration = absoluteDuration;
        }

        public DictionaryCachedItem(Exception exception, TimeSpan duration, TimeSpan absoluteDuration)
        {
            ThrownException = exception;
            Created = DateTime.Now;

            Duration = duration;
            AbsoluteDuration = absoluteDuration;
        }
    }
}