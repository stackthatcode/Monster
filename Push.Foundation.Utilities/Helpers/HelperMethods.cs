namespace Push.Foundation.Utilities.Helpers
{
    public static class HelperMethods
    {
        public static string IfNotNullEmpty(this string input, string output)
        {
            return !input.IsNullOrEmpty() ? output : "";
        }


        public static string WrapInParagraph(this string input)
        {
            return $"<p>{input}</p>";
        }

        public static string IfNotNullEmptyAppend(this string input, string addition)
        {
            return !addition.IsNullOrEmpty() ? input + addition : input;
        }

        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string IsNullOrEmptyAlt(this string input, string alternative)
        {
            return string.IsNullOrEmpty(input) ? alternative : input;
        }

        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}

