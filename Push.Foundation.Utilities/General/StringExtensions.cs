using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Foundation.Utilities.General
{
    public static class StringExtensions
    {
        public static string TruncateAfter(this string input, int length, string terminator = "")
        {
            return input.Length <= length ? input : input.Substring(0, length) + terminator;
        }

        public static string JoinByNewline(this IEnumerable<string> input)
        {
            return string.Join(Environment.NewLine, input);
        }

        public static string ToCommaDelimited(this IEnumerable<string> input)
        {
            return string.Join(",", input);
        }
        
        public static string ToDelimited(this IEnumerable<string> input, string delimeter)
        {
            return string.Join(delimeter, input);
        }

        public static string ToNewlineDelimited(this IEnumerable<string> input)
        {
            return string.Join(Environment.NewLine, input);
        }


        public static bool CaselessEquals(this string input, string other)
        {
            return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
        }

        public static string LetterOrNumbersOnly(this string input)
        {
            return new string(input.Select(x => Char.IsLetterOrDigit(x) ? x : '_').ToArray());
        }
    }
}