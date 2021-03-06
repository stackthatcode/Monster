﻿using Newtonsoft.Json;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Testbed.ShopifyFeeder
{
    public class OrderPayload
    {
        public string CustomerEmail { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        
        public long LineItemVariantId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        [JsonIgnore]
        public string CustomerJson => new
        {
            customer = new
            {
                first_name = CustomerFirstName,
                last_name = CustomerLastName,
                email = CustomerEmail,
            }
        }.SerializeToJson();
        
        public string ToOrder(long customerId)
        {
            return new
            {
                order = new
                {
                    suppress_notifications = true,
                    customer = new
                    {
                        id = customerId,
                    },
                    send_receipt = false,
                    send_fulfillment_receipt = false,
                    line_items = new object[]
                    {
                        new
                        {
                            variant_id = LineItemVariantId,
                            quantity = Quantity,
                        },
                    },
                    transactions = new object[]
                    {
                        new
                        {
                            kind =  TransactionKind.Sale,
                            status = TransactionStatus.Success,
                            gateway = "bogus",
                            amount = Quantity * UnitPrice,
                        }
                    }
                }
            }.SerializeToJson();
        }

        public string ToOrderTransaction()
        {
            return new
            {
                transaction = new
                {
                    currency = "USD",
                    amount = Quantity * UnitPrice,
                    kind = TransactionKind.Capture,
                    test = true,
                    gateway = "bogus",
                    parent_id = 389404469,
                }
            }.SerializeToJson();
        }
    }
}
