using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheConfiguration
{
    public static class Param
    {
        public static TResult IsAny<TResult>()
        {
            return default(TResult);
        }

        public static TResult IsBetween<TResult>(TResult lower, TResult upper)
        {
            return default(TResult);
        }
    }
}
