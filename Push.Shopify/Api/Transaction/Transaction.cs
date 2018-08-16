using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Transaction
{
    public class Transaction
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public decimal amount { get; set; }
        public string kind { get; set; }
        public string gateway { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public DateTimeOffset? created_at { get; set; }
        public bool test { get; set; }
        public string authorization { get; set; }
        public string currency { get; set; }
        public string location_id { get; set; }
        public string user_id { get; set; }
        public long? parent_id { get; set; }
        public string device_id { get; set; }
        public Receipt receipt { get; set; }
        public string error_code { get; set; }
        public string source_name { get; set; }

        // Credit Card-specific details
        public PaymentDetails payment_details { get; set; }
    }

    public class TransactionList
    {
        public List<Transaction> transactions { get; set; }

        [JsonIgnore]
        public IEnumerable<Transaction> Successful
                => transactions.ByStatus(TransactionStatus.Success);

        [JsonIgnore]
        public IEnumerable<Transaction> Captured
                => Successful.ByKind(new[] {TransactionKind.Capture, TransactionKind.Sale});


        public bool IsVoided 
                => Successful.ByKind(TransactionKind.Void).Any();

        public bool IsPaymentCaptured => Captured.Any() && !this.IsVoided;

        public decimal TotalCaptured => Captured.Sum(x => x.amount);

        public decimal TotalRefunded => 
                Successful.ByKind(TransactionKind.Refund).Sum(x => x.amount);

        public decimal NetTotal => IsVoided ? 0 : TotalCaptured - TotalRefunded;
    }
}
