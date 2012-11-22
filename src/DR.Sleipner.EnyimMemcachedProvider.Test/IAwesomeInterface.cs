using System.Collections.Generic;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    public interface IAwesomeInterface
    {
        void VoidMethod();
        IEnumerable<string> ParameterlessMethod();
        IEnumerable<string> ParameteredMethod(string a, int b);
    }
}