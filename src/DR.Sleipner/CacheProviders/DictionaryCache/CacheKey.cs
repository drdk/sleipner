using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class CacheKey
    {
        private readonly object _primaryKey;
        private readonly object[] _cacheKeys;
        public CacheKey(object primaryKey, object[] cacheKeys)
        {
            _primaryKey = primaryKey;
            _cacheKeys = cacheKeys;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.);
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to this type return false.
            var otherKey = obj as CacheKey;

            return Equals(otherKey);
        }

        public bool Equals(CacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(other._primaryKey, _primaryKey) && _cacheKeys.SequenceEqual(other._cacheKeys);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_primaryKey != null ? _primaryKey.GetHashCode() : 0)*397) ^ (_cacheKeys != null ? _cacheKeys.GetHashCode() : 0);
            }
        }
    }
}
