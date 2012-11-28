using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders.DictionaryCache
{
    public class DictionaryCacheKey
    {
        private readonly MethodInfo _primaryKey;
        private readonly object[] _cacheKeys;
        public DictionaryCacheKey(MethodInfo primaryKey, object[] cacheKeys)
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
            var otherKey = obj as DictionaryCacheKey;

            return Equals(otherKey);
        }

        public bool Equals(DictionaryCacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(other._primaryKey, _primaryKey) && _cacheKeys.SequenceEqual(other._cacheKeys, new CollectionComparer());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _primaryKey.GetHashCode()*397;
            }
        }
    }
}
