using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public static class Extensions
    {
        public const int FudgeFactorMinutes = -10;

        public static DateTime AddAcumaticaBatchFudge(this DateTime input)
        {
            return input.AddMinutes(FudgeFactorMinutes);
        }

        public static bool IsReadyForShipment(this UsrAcumaticaSalesOrder order)
        {
            return order.AcumaticaStatus == "Open" || 
                   order.AcumaticaStatus == "Back Order";
        }
        
        public static bool IsMatchedToShopify(this AcumaticaStockItem stockItem)
        {
            return stockItem.ShopAcuItemSyncs.Count != 0;
        }

        public static AcumaticaWarehouse ByDetail(
                    this List<AcumaticaWarehouse> input, 
                    AcumaticaWarehouseDetail detail)
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


        public static AcumaticaWarehouseDetail
                WarehouseDetail(
                    this AcumaticaStockItem input,
                    string warehouseId)
        {
            return input
                .AcumaticaWarehouseDetails
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


        public static SalesOrder ToSalesOrderObj(this string json)
        {
            return json.DeserializeFromJson<SalesOrder>();
        }

        public static SalesInvoice ToSalesOrderInvoiceObj(this string json)
        {
            return json.DeserializeFromJson<SalesInvoice>();
        }

    }
}
