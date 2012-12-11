using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.Test.TestModel
{
    public interface IAwesomeInterface
    {
        void VoidMethod();
        IEnumerable<string> FaulyCachedMethod();
        IEnumerable<string> FaulyNonCachedMethod();
        IEnumerable<string> NonCachedMethod();
        IEnumerable<string> ParameterlessMethod();
        IEnumerable<string> DatedMethod(int a, DateTime time);
        IEnumerable<string> ParameteredMethod(string a, int b);
        IEnumerable<string> ParameteredMethod(string a, int b, IList<string> list);
        IEnumerable<string> EnumMethod(AwesomeEnum level, string a);
        IList<T> GenericMethod<T>(string str, int number);
        IDictionary<TKey, TValue> StrangeGenericMethod<TValue, TKey>(TKey keys, IEnumerable<TValue> values); 
        object LolMethod();
        int RoflMethod();
    }
}