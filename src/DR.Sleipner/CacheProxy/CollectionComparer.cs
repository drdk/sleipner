using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy
{
    public class CollectionComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (x.Equals(y))
                return true;

            //If we didn't return true before this, we're going to check if both objects are IEnumerable
            var xEnum = x as IEnumerable;
            var yEnum = y as IEnumerable;

            if (xEnum == null || yEnum == null)
                return false;

            var xEnumCast = xEnum.Cast<object>();
            var yEnumCast = yEnum.Cast<object>();

            var squenceEquals = xEnumCast.SequenceEqual(yEnumCast, new CollectionComparer());
            return squenceEquals;
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}