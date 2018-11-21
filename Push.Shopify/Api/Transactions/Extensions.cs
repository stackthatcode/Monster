using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Transactions
{
    public static class Extensions
    {
        public static IEnumerable<Transactions.Transaction> 
                    ByStatus(this IEnumerable<Transactions.Transaction> transactions, string[] statuses)
        {
            return transactions.Where(x => statuses.Any(status => status == x.status));
        }

        public static IEnumerable<Transactions.Transaction>
                    ByStatus(this IEnumerable<Transactions.Transaction> transactions, string status)
        {
            return transactions.ByStatus(new[] {status});
        }
        


        public static IEnumerable<Transactions.Transaction>
                    ByKind(this IEnumerable<Transactions.Transaction> transactions, string[] kinds)
        {
            return transactions.Where(x => kinds.Any(kind => kind == x.kind));
        }

        public static IEnumerable<Transactions.Transaction>
                    ByKind(this IEnumerable<Transactions.Transaction> transactions, string kind)
        {
            return transactions.ByStatus(new[] { kind });
        }
    }
}

