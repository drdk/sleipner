using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config.Parsers
{
    public class ConstantParser : IParameterParser
    {
        private readonly object _constant;

        public ConstantParser(object constant)
        {
            _constant = constant;
        }

        public bool IsMatch(object value)
        {
            return Equals(value, _constant);
        }
    }
}
