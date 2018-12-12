using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public static class Extensions
    {
        public static bool IsReadyForShipment(this UsrAcumaticaSalesOrder order)
        {
            return order.AcumaticaStatus == "Open" || order.AcumaticaStatus == "Back Order";
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
                    this IEnumerable<UsrAcumaticaShipmentSalesOrderRef> input,
                    UsrAcumaticaShipmentSalesOrderRef other)
        {
            return input.Any(x => x.Match(other));
        }

        public static bool Match(
                this UsrAcumaticaShipmentSalesOrderRef input, UsrAcumaticaShipmentSalesOrderRef other)
        {
            return input.AcumaticaOrderNbr == other.AcumaticaOrderNbr
                   && input.AcumaticaShipmentNbr == other.AcumaticaShipmentNbr;
        }

        public static UsrAcumaticaShipmentSalesOrderRef FindMatch(
                this IEnumerable<UsrAcumaticaShipmentSalesOrderRef> input,
                UsrAcumaticaShipmentSalesOrderRef findMe)
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

        public static List<string> 
                    UniqueOrderNbrs(this UsrAcumaticaShipment input)
        {
            return input
                    .UsrAcumaticaShipmentSalesOrderRefs
                    .Select(x => x.AcumaticaOrderNbr)
                    .ToList();
        }

        public static Shipment ToAcuObject(this UsrAcumaticaShipment input)
        {
            return input.AcumaticaJson.DeserializeFromJson<Shipment>();
        }

        public static SalesOrder ToAcuObject(this UsrAcumaticaSalesOrder input)
        {
            return input.DetailsJson.DeserializeFromJson<SalesOrder>();
        }
    }
}
