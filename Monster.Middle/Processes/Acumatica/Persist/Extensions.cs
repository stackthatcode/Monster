using System;
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
        public const int PullEndFudgeMinutes = -10;

        public static DateTime AddAcumaticaBatchFudge(this DateTime input)
        {
            return input.AddMinutes(PullEndFudgeMinutes);
        }

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
            return input.Any(x => x.IsMatch(other));
        }
        public static UsrAcumaticaShipmentSalesOrderRef Match(
            this IEnumerable<UsrAcumaticaShipmentSalesOrderRef> input,
            UsrAcumaticaShipmentSalesOrderRef other)
        {
            return input.FirstOrDefault(x => x.IsMatch(other));
        }

        public static bool IsMatch(
                this UsrAcumaticaShipmentSalesOrderRef input, UsrAcumaticaShipmentSalesOrderRef other)
        {
            return input.AcumaticaOrderNbr == other.AcumaticaOrderNbr
                   && input.AcumaticaShipmentNbr == other.AcumaticaShipmentNbr;
        }

        public static UsrAcumaticaShipmentSalesOrderRef FindMatch(
                this IEnumerable<UsrAcumaticaShipmentSalesOrderRef> input,
                UsrAcumaticaShipmentSalesOrderRef findMe)
        {
            return input.FirstOrDefault(x => x.IsMatch(findMe));
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



        public static bool AnyMatch(
            this IEnumerable<UsrAcumaticaSoShipmentInvoice> input,
            UsrAcumaticaSoShipmentInvoice other)
        {
            return input.Any(x => x.IsMatch(other));
        }

        public static UsrAcumaticaSoShipmentInvoice Match(
            this IEnumerable<UsrAcumaticaSoShipmentInvoice> input,
            UsrAcumaticaSoShipmentInvoice other)
        {
            return input.FirstOrDefault(x => x.IsMatch(other));
        }
        
        public static bool IsMatch(
            this UsrAcumaticaSoShipmentInvoice input, UsrAcumaticaSoShipmentInvoice other)
        {
            return input.AcumaticaShipmentNbr == other.AcumaticaShipmentNbr;
        }

    }
}
