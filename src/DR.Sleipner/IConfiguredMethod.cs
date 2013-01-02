using System.Collections.Generic;
using System.Reflection;

namespace DR.Sleipner
{
    public interface IConfiguredMethod<T> where T : class
    {
        bool IsMatch(MethodInfo method, IEnumerable<object> arguments);
    }
}