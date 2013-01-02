using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config
{
    public interface IMethodFamilyConfigurationExpression
    {
        IMethodFamilyConfigurationExpression CacheFor(int duration);
        IMethodFamilyConfigurationExpression ExpireAfter(int maxDuration);
        void DisableCache();
        IMethodFamilyConfigurationExpression SupressExceptionsWhenStale();
        IMethodFamilyConfigurationExpression BubbleExceptionsWhenStale();
    }
}
