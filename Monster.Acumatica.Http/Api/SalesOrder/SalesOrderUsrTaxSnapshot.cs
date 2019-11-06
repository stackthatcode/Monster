namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderUsrTaxSnapshot
    {
        public class CustomDocument
        {
            public CustomField UsrTaxSnapshot { get; set; }
        }

        public class CustomField
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public CustomDocument Document { get; set; }

        public SalesOrderUsrTaxSnapshot(string base64ZippedTaxTransfer)
        {

            Document = new CustomDocument()
            {
                UsrTaxSnapshot = new CustomField
                {
                    type = "CustomStringField",
                    value = base64ZippedTaxTransfer,
                }
            };
        }
    }
}
