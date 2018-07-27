using System;

namespace Push.Foundation.Utilities.Helpers
{
    public static class NumberHelpers
    {
        public static bool IsDecimal(this string input, int? min = null, int? max = null)
        {
            decimal output;
            var result = decimal.TryParse(input, out output);

            if (!result)
            {
                return false;
            }
            if (min.HasValue && output < min.Value)
            {
                return false;
            }
            if (max.HasValue && output > max.Value)
            {
                return false;
            }
            return true;
        }

        public static bool IsInteger(this string input)
        {
            int dummy;
            return Int32.TryParse(input, out dummy);
        }

        public static int? ToNullableInteger(this string input)
        {
            return input.IsInteger() ? int.Parse(input) : (int?)null;
        }
        
        public static decimal ToDecimal(this string input)
        {
            return decimal.Parse(input);
        }

        public static int ToInteger(this string input)
        {
            return int.Parse(input);
        }

        public static int ToIntegerAlt(this string input, int altValue)
        {
            return input.IsInteger() ? int.Parse(input) : altValue;
        }


        public static long ToLong(this string input)
        {
            return long.Parse(input);
        }

        //public static decimal? ToDecimalFromPercentage
    }
}

