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
        private readonly IDictionary<RequestKey, object> _returnValues = new ConcurrentDictionary<RequestKey, object>();
 
        public Func<TResult> GetWaitFunction<TResult>(RequestKey key)
        {
            lock (this)
            {
                ManualResetEvent resetEvent;
                if (_resetEvents.TryGetValue(key, out resetEvent))
                {
                    return () =>
                               {
                                   resetEvent.WaitOne();
                                   return (TResult)_returnValues[key];
                               };
                }

                _resetEvents[key] = new ManualResetEvent(false);

                return null;
            }
        }

        public void Release<TResult>(RequestKey key, TResult result)
        {
            _returnValues[key] = result;
            _resetEvents[key].Set();
            _resetEvents.Remove(key);
        }
    }
}