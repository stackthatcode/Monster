﻿using System;
using Monster.Acumatica.Api.SalesOrder;
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
