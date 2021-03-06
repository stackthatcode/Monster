﻿using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Misc.State
{
    public static class StateExtensions
    {
        public static T ScalarQuery<T>(this DbContext input, string sql)
        {
            return input.Database.SqlQuery<T>(sql).FirstOrDefault();
        }

        public static bool IsShopifyUrlFinalized(this SystemState state)
        {
            return state.ShopifyConnState != StateCode.None;
        }

        public static bool CanPollDataFromShopify(this SystemState state)
        {
            return state.ShopifyConnState == StateCode.Ok;
        }

        public static bool CanPollDataFromAcumatica(this SystemState state)
        {
            return state.AcumaticaConnState == StateCode.Ok;
        }

        public static bool CanSyncOrdersToAcumatica(this SystemState state)
        {
            return state.AcumaticaConnState == StateCode.Ok &&
                   state.AcumaticaRefDataState == StateCode.Ok &&
                   state.WarehouseSyncState == StateCode.Ok;
        }

        public static bool CanSyncRefundsToAcumatica(this SystemState state)
        {
            return state.CanSyncOrdersToAcumatica();
        }

        public static bool CanSyncFulfillmentsToShopify(this SystemState state)
        {
            return state.ShopifyConnState == StateCode.Ok &&
                   state.InventoryRefreshState == StateCode.Ok;
        }

        public static bool CanSyncInventoryCountsToShopify(this SystemState state)
        {
            return state.ShopifyConnState == StateCode.Ok &&
                   state.InventoryRefreshState == StateCode.Ok;
        }
    }
}
