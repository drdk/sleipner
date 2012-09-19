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

        public bool IsExpired
        {
            get { return Created + Duration < DateTime.Now; }
        }

        public DictionaryCachedItem(object obj, TimeSpan duration)
        {
            Object = obj;
            Created = DateTime.Now;
            Duration = duration;
        }

        public DictionaryCachedItem(Exception exception, TimeSpan duration)
        {
            ThrownException = exception;
            Created = DateTime.Now;
            Duration = duration;
        }
    }
}