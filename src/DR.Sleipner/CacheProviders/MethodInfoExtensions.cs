using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders
{
    public static class MethodInfoExtensions
    {
        public static CacheBehaviorAttribute FindCacheBehavior(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            var searchOrder = new Func<IEnumerable<CacheBehaviorAttribute>>[]
                                  {
                                      () => methodInfo.GetCustomAttributes(true).OfType<CacheBehaviorAttribute>(),
                                      () =>
                                          {
                                              //Find this method on one of the interfaces implemented by this class
                                              var interfaceMethodInfo = methodInfo.DeclaringType.GetInterfaces().Select(x => x.GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(n => n.ParameterType).ToArray())).Single();
                                              return interfaceMethodInfo == null ? Enumerable.Empty<CacheBehaviorAttribute>() : interfaceMethodInfo.GetCustomAttributes(true).OfType<CacheBehaviorAttribute>();
                                          },
                                      () => methodInfo.DeclaringType == null ? Enumerable.Empty<CacheBehaviorAttribute>() : methodInfo.DeclaringType.GetCustomAttributes(true).OfType<CacheBehaviorAttribute>(),
                                      () =>
                                          {
                                              var interfaces = methodInfo.DeclaringType.GetInterfaces();
                                              var correctInterface = interfaces.Single(a => a.GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(b => b.ParameterType).ToArray()) != null);
                                              return correctInterface.GetCustomAttributes(true).OfType<CacheBehaviorAttribute>();
                                          }
                                  };

            var cacheAttribute = searchOrder.SelectMany(a => a()).FirstOrDefault(); //This should iterate over the collection of search delegates until one of the yields something. I think?
            
            return cacheAttribute;
        }
    }
}
