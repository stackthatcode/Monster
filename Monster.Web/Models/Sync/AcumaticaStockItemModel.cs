using System;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Web.Models.Sync
{
    public class AcumaticaStockItemModel
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public string AcumaticaUrl { get; set; }
        public int QuantityOnHand { get; set; }
        
        public static AcumaticaStockItemModel Make(
                AcumaticaStockItem stockItem, Func<string, string> stockItemUrlBuilder)
        {
            var output = new AcumaticaStockItemModel();
            output.ItemId = stockItem.ItemId;
            output.Description = stockItem.AcumaticaDescription;
            output.AcumaticaUrl = stockItemUrlBuilder(stockItem.ItemId);
            output.QuantityOnHand = 
                (int)stockItem.AcumaticaWarehouseDetails.Sum(x => x.AcumaticaQtyOnHand);

            return output;
        }
    }
}
