using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Push.Foundation.Utilities.General
{
    public static class LambdaExtensions
    {
        public static void SetPropertyValue<T, TValue>(
                this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(target, value, null);
                }
            }
        }

        public static string MemberName<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                return null;

            return memberExpression.Member.Name;
        }

        public static string UnaryMethodName<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            var convert = expression.Body as MethodCallExpression;
            return convert.Method.Name;
        }
    }
}
