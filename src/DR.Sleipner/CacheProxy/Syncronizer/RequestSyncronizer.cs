using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DR.Sleipner.CacheProxy.Syncronizer
{
    public class RequestSyncronizer : IRequestSyncronizer
    {
        private readonly IDictionary<RequestKey, ManualResetEvent> _resetEvents = new ConcurrentDictionary<RequestKey, ManualResetEvent>(); 
        private readonly object _padLock = new object();

        public Action GetWaitFunction(RequestKey key)
        {
            lock (this)
            {
                ManualResetEvent resetEvent;
                if (_resetEvents.TryGetValue(key, out resetEvent))
                {
                    return () => resetEvent.WaitOne();
                }

                _resetEvents[key] = new ManualResetEvent(false);

                return null;
            }
        }

        public void Release(RequestKey key)
        {
            _resetEvents[key].Set();
            _resetEvents.Remove(key);
        }
    }
}