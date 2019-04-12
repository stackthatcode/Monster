using System;
using System.Web;

namespace Push.Foundation.Utilities.Http
{
    public static class UrlExtensions
    {
        public static string ExtractQueryString(this string input)
        {
            if (input == null)
            {
                return null;
            }
            var index = input.IndexOf("?", StringComparison.Ordinal);
            if (index == -1)
            {
                return null;
            }
            var queryString = input.Substring(index);
            return queryString;
        }

        public static string ExtractQueryParameter(this string input, string name)
        {
            var queryString = input.ExtractQueryString();
            if (queryString == null)
            {
                return "";
            }
            else
            {
                var queryStringCollection = HttpUtility.ParseQueryString(queryString);
                return queryStringCollection[name];
            }
        }

        public static string UrlEncode(this string input)
        {
            return HttpUtility.UrlEncode(input);
        }
    }
}
