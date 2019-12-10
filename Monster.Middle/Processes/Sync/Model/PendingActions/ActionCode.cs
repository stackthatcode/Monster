using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public enum ActionCode
    {
         None = 1,
         CreateInAcumatica = 2,
         CreateBlankSyncRecord = 3,
         ReleaseInAcumatica = 4,
         UpdateInAcumatica = 5,
         CreateInShopify = 6,
    }

    public static class ActionCodeDesc
    {
        private static readonly
            Dictionary<ActionCode, string> Lookup = new Dictionary<ActionCode, string>()
            {
                {ActionCode.None, "No actions pending"},
                {ActionCode.CreateInAcumatica, "Pending creation in Acumatica"},
                {ActionCode.ReleaseInAcumatica, "Pending release in Acumatica"},
                {ActionCode.UpdateInAcumatica, "Pending update in Acumatica"},
                {ActionCode.CreateInShopify, "Pending creation in Shopify"}
            };

        public static string Description(this ActionCode actionCode)
        {
            return Lookup[actionCode];
        }
    }
}
