using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Transaction
{
    public static class Extensions
    {
        public static IEnumerable<Transaction> 
                    ByStatus(this IEnumerable<Transaction> transactions, string[] statuses)
        {
            return transactions.Where(x => statuses.Any(status => status == x.status));
        }

        public static IEnumerable<Transaction>
                    ByStatus(this IEnumerable<Transaction> transactions, string status)
        {
            return transactions.ByStatus(new[] {status});
        }
        


        public static IEnumerable<Transaction>
                    ByKind(this IEnumerable<Transaction> transactions, string[] kinds)
        {
            return transactions.Where(x => kinds.Any(kind => kind == x.kind));
        }

        public static IEnumerable<Transaction>
                    ByKind(this IEnumerable<Transaction> transactions, string kind)
        {
            return transactions.ByStatus(new[] { kind });
        }
    }
}

