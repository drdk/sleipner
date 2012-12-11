using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config.Parsers
{
    public class AnyValueParser : IParameterParser
    {
        public bool IsMatch(object value)
        {
            return true;
        }
    }
}