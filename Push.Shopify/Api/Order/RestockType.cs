using System;
using System.Collections.Generic;


namespace Push.Shopify.Api.Order
{
    public class RestockType
    {
        public const int NoRestock = 1;
        public const int Cancel = 2;
        public const int Return = 3;
    }

    public static class RestockTypeExtensions
    {
        private static readonly
                Dictionary<string, int>
                    _storage = new Dictionary<string, int>
                {
                    { "no_restock", RestockType.NoRestock },
                    { "cancel", RestockType.Cancel },
                    { "return", RestockType.Return },
                };

        public static int ToRestockType(this string input)
        {
            if (_storage.ContainsKey(input))
            {
                return _storage[input];
            }
            throw new ArgumentException($"Unrecognized restock_type: {input}");
        }
    }
}
