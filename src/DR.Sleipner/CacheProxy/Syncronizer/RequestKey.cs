using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheProxy.Syncronizer
{
    public class RequestKey : IEquatable<RequestKey>
    {
        private readonly MethodInfo _methodInfo;
        private readonly object[] _parameters;

        public RequestKey(MethodInfo methodInfo, object[] parameters)
        {
            _methodInfo = methodInfo;
            _parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RequestKey)obj);
        }

        public bool Equals(RequestKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var one = _methodInfo.Equals(other._methodInfo);
            var two = _parameters.SequenceEqual(other._parameters);

            var res = one && two;
            return res;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_methodInfo.GetHashCode()*397);
            }
        }
    }
}
