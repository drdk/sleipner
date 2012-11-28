using System.Collections.Generic;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    public interface IAwesomeInterface
    {
        void VoidMethod();
        IEnumerable<string> ParameterlessMethod();
        IEnumerable<string> ParameteredMethod(string a, int b);
        IEnumerable<string> ParameteredMethod(string a, int b, IList<string> list);
        IEnumerable<string> ParameteredMethod(string a, int b, IList<int> list);
        IEnumerable<string> ParameteredMethod(string a, int b, IList<object> list);
        IEnumerable<string> EnumMethod(AwesomeEnum level, string a);
    }
}