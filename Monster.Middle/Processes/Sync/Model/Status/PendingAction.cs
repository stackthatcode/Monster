using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public enum PendingAction
    {
         None = 1,
         CreateInAcumatica = 2,
         ReleaseInAcumatica = 3,
         UpdateInAcumatica = 4,
         CreateInShopify = 5,
    }

    public static class PendingActionDesc
    {
        private static readonly
            Dictionary<PendingAction, string> Lookup = new Dictionary<PendingAction, string>()
            {
                {PendingAction.None, "No actions pending"},
                {PendingAction.CreateInAcumatica, "Pending creation in Acumatica"},
                {PendingAction.ReleaseInAcumatica, "Pending release in Acumatica"},
                {PendingAction.UpdateInAcumatica, "Pending update in Acumatica"},
                {PendingAction.CreateInShopify, "Pending creation in Shopify"}
            };

        public static string Description(this PendingAction action)
        {
            return Lookup[action];
        }
    }
}
