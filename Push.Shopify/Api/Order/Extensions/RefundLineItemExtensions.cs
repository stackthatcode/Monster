using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Order.Extensions
{


    public static class RefundLineItemExtensions
    {
        private static readonly 
                Dictionary<string, int> _storage = 
                    new Dictionary<string, int>()
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
            throw new ArgumentException($"Do not recognize restock_type {input}");
        }
    }
}
