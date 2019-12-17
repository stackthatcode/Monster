using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class Address
    {
        public StringValue AddressLine1 { get; set; }
        public StringValue AddressLine2 { get; set; }
        public StringValue City { get; set; }
        public StringValue State { get; set; }
        public StringValue PostalCode { get; set; }
        public StringValue CountryID;
    }
}
