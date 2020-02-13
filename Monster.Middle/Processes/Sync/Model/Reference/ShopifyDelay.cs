using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Reference
{
    public class ShopifyDelay
    {
        public static int Default => 1000;
        public static List<int> Data => new List<int>()  { 0, 500, 1000, 2000 };
    }
}
