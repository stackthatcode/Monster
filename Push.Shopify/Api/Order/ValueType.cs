using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Order
{
    public class ValueType
    {
        public const int FixedAmount = 1;
        public const int Percentage = 2;
    }

    public static class ValueTypeExtensions
    {
        private static readonly
                Dictionary<string, int>
                    _storage = new Dictionary<string, int>
                {
                    { "fixed_amount", ValueType.FixedAmount },
                    { "percentage", ValueType.Percentage },
                };

        public static int ToValueType(this string input)
        {
            if (_storage.ContainsKey(input))
            {
                return _storage[input];
            }
            throw new ArgumentException($"Unrecognized restock_type: {input}");
        }
    }
}
