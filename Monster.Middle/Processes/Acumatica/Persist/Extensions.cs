using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
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

        
        public static bool IsMatchedToShopify(this AcumaticaStockItem stockItem)
        {
            return stockItem.ShopAcuItemSyncs.Count != 0;
        }

        public static AcumaticaWarehouse ByDetail(
                    this List<AcumaticaWarehouse> input, AcumaticaWarehouseDetail detail)
        {
            return input.FirstOrDefault(x => x.AcumaticaWarehouseId == detail.AcumaticaWarehouseId);
        }


        public static AcumaticaWarehouseDetail
                    WarehouseDetail(this AcumaticaStockItem input, string warehouseId)
        {
            return input
                .AcumaticaWarehouseDetails
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouseId);
        }

        public static SalesOrder ToSalesOrderObj(this AcumaticaSalesOrder orderRecord)
        {
            return orderRecord.AcumaticaShipmentDetailsJson.ToSalesOrderObj();
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
