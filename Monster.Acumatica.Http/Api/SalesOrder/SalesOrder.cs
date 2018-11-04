﻿using System.Collections.Generic;
using Monster.Acumatica.Api.Common;


namespace Monster.Acumatica.Api.SalesOrder
{

    public class SalesOrder
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public BoolValue Approved { get; set; }
        public StringValue BaseCurrencyID { get; set; }
        public BoolValue BillToAddressOverride { get; set; }
        public BoolValue BillToContactOverride { get; set; }
        public StringValue CashAccount { get; set; }
        public DoubleValue ControlTotal { get; set; }
        public BoolValue CreditHold { get; set; }
        public StringValue CurrencyID { get; set; }
        public DoubleValue CurrencyRate { get; set; }
        public StringValue CurrencyRateTypeID { get; set; }
        public StringValue CustomerID { get; set; }
        public StringValue CustomerOrder { get; set; }
        public DateValue Date { get; set; }
        public StringValue Description { get; set; }
        public StringValue DestinationWarehouseID { get; set; }
        public List<SalesOrderDetail> Details { get; set; }
        public DateValue EffectiveDate { get; set; }
        public StringValue ExternalRef { get; set; }
        public BoolValue Hold { get; set; }
        public BoolValue IsTaxValid { get; set; }
        public DateValue LastModified { get; set; }
        public BoolValue NewCard { get; set; }
        public DoubleValue OrderedQty { get; set; }
        public StringValue OrderNbr { get; set; }
        public DoubleValue OrderTotal { get; set; }
        public StringValue OrderType { get; set; }
        public StringValue PaymentCardIdentifier { get; set; }
        public StringValue PaymentMethod { get; set; }
        public StringValue PaymentRef { get; set; }
        public StringValue PreAuthorizationNbr { get; set; }
        public StringValue PreAuthorizedAmount { get; set; }
        public StringValue PreferredWarehouseID { get; set; }
        public DoubleValue ReciprocalRate { get; set; }
        public DateValue RequestedOn { get; set; }
        public BoolValue ShipToAddressOverride { get; set; }
        public BoolValue ShipToContactOverride { get; set; }
        public StringValue ShipVia { get; set; }
        public StringValue Status { get; set; }
        public DoubleValue TaxTotal { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}