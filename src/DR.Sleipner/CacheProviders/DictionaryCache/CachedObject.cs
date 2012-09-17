using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class CachedObject
    {
        public readonly object Object;
        public readonly DateTime Created;
        public readonly TimeSpan Duration;

        public bool IsExpired
        {
            get { return Created + Duration < DateTime.Now; }
        }

        public CachedObject(object obj, TimeSpan duration)
        {
            Object = obj;
            Created = DateTime.Now;
            Duration = duration;
        }
    }
}