using System;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.Model
{
    public class CachedObject<T>
    {
        public CachedObjectState State { get; private set; }
        public T Object { get; private set; }
        public Exception ThrownException { get; private set; }

        public CachedObject(CachedObjectState state, T obj)
        {
            State = state;
            Object = obj;
        }

        public CachedObject(CachedObjectState state, Exception exception)
        {
            State = state;
            ThrownException = exception;
        }
    }
}
