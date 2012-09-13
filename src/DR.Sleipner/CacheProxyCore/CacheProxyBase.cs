using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxyCore
{
    public abstract class CacheProxyBase<T> where T : class
    {
        public T RealInstance;

        public CacheProxyBase(T real)
        {
            RealInstance = real;
        }

        public object GetCachedItem(string methodName, object[] parameters)
        {
            Console.WriteLine("Intercept: " + methodName + "(" + string.Join(", ", parameters) + ")");

            return null;
        }

        public void StoreItem(string methodName, object[] parameters, object item)
        {
            Console.WriteLine("StoreItem: " + methodName + "(" + string.Join(", ", parameters) + ", " + item + ")");
        }
    }
}