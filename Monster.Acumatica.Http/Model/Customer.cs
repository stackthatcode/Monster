using System;
using System.Collections.Generic;

namespace Monster.Acumatica.Model
{
    public class CustomerList
    {
        public List<Customer> Customers { get; set; }
    }

    public class Customer
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }

        public StringValue CustomerID { get; set; }

        public Contact MainContact { get; set; }
        public Contact BillingContact { get; set; }
        public Contact ShippingContact { get; set; }


        public StringValue AccountRef { get; set; }
        public BoolValue ApplyOverdueCharges { get; set; }
        public BoolValue AutoApplyPayments { get; set; }
        public BoolValue BillingAddressSameAsMain { get; set; }
        public BoolValue BillingContactSameAsMain { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue CurrencyID { get; set; }
        public StringValue CurrencyRateType { get; set; }
        public StringValue CustomerClass { get; set; }
        public StringValue CustomerName { get; set; }
        public BoolValue EnableCurrencyOverride { get; set; }
        public BoolValue EnableRateOverride { get; set; }
        public BoolValue EnableWriteOffs { get; set; }
        public StringValue FOBPoint { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue LeadTimedays { get; set; }
        public BoolValue MultiCurrencyStatements { get; set; }
        public IntegerValue OrderPriority { get; set; }
        public StringValue ParentRecord { get; set; }
        public StringValue PriceClassID { get; set; }
        public BoolValue PrintDunningLetters { get; set; }
        public BoolValue PrintInvoices { get; set; }
        public BoolValue PrintStatements { get; set; }
        public BoolValue ResidentialDelivery { get; set; }
        public BoolValue SaturdayDelivery { get; set; }
        public BoolValue SendDunningLettersbyEmail { get; set; }
        public BoolValue SendInvoicesbyEmail { get; set; }
        public BoolValue SendStatementsbyEmail { get; set; }
        public BoolValue ShippingAddressSameAsMain { get; set; }
        public BoolValue ShippingContactSameAsMain { get; set; }
        public StringValue ShippingRule { get; set; }
        public StringValue ShippingTerms { get; set; }
        public StringValue ShippingZoneID { get; set; }
        public StringValue ShipVia { get; set; }
        public StringValue StatementCycleID { get; set; }
        public StringValue StatementType { get; set; }
        public StringValue Status { get; set; }
        public StringValue TaxRegistrationID { get; set; }
        public StringValue TaxZone { get; set; }
        public StringValue Terms { get; set; }
        public DoubleValue WriteOffLimit { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
