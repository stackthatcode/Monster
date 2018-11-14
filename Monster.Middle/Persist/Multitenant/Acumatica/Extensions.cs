using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Acumatica
{
    public static class Extensions
    {
        public static bool IsReadyForShipment(this UsrAcumaticaSalesOrder order)
        {
            return order.AcumaticaStatus == "Open";
        }
        
        public static bool IsMatchedToShopify(this UsrAcumaticaStockItem stockItem)
        {
            return stockItem.UsrShopAcuItemSyncs.Count != 0;
        }

        public static UsrAcumaticaWarehouse ByDetail(
                    this List<UsrAcumaticaWarehouse> input, 
                    UsrAcumaticaWarehouseDetail detail)
        {
            return input.FirstOrDefault(
                    x => x.AcumaticaWarehouseId == detail.AcumaticaWarehouseId);
        }


        public static bool AnyMatch(
                    this IEnumerable<UsrAcumaticaShipmentSo> input,
                    UsrAcumaticaShipmentSo other)
        {
            return input.Any(x => x.Match(other));
        }

        public static bool Match(
                this UsrAcumaticaShipmentSo input, UsrAcumaticaShipmentSo other)
        {
            return input.AcumaticaOrderNbr == other.AcumaticaOrderNbr
                   && input.AcumaticaShipmentNbr == other.AcumaticaShipmentNbr;
        }

        public static UsrAcumaticaShipmentSo FindMatch(
                this IEnumerable<UsrAcumaticaShipmentSo> input,
                UsrAcumaticaShipmentSo findMe)
        {
            return input.FirstOrDefault(x => x.Match(findMe));
        }


        public static UsrAcumaticaWarehouseDetail
                WarehouseDetail(
                    this UsrAcumaticaStockItem input,
                    string warehouseId)
        {
            return input
                .UsrAcumaticaWarehouseDetails
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouseId);
        }

    }
}
