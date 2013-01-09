using System;

namespace DR.Sleipner.Caching.Model
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

        public T GetObject()
        {
            if(ThrownException != null)
            {
                throw ThrownException;
            }

            return Object;
        }
    }
}
