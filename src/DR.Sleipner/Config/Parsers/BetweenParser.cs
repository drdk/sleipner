using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Config.Parsers
{
    public class BetweenParser : IParameterParser
    {
        private readonly Func<IComparable> _lower;
        private readonly Func<IComparable> _upper;

        public BetweenParser(Func<IComparable> lower, Func<IComparable> upper)
        {
            _lower = lower;
            _upper = upper;
        }

        public bool IsMatch(object value)
        {
            var lower = _lower();
            var upper = _upper();

            var a = lower.CompareTo(value);
            var b = upper.CompareTo(value);

            return a <= 0
                && b >= 0;
        }
    }
}