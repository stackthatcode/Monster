using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Customer
{
    public class Contact
    {
        // GUID?
        public string id { get; set; }
        public int rowNumber { get; set; }
        public object note { get; set; }
        public StringValue ContactID { get; set; }
        public StringValue DisplayName { get; set; }
        public StringValue Email { get; set; }
        public StringValue Fax { get; set; }
        public StringValue FirstName { get; set; }
        public StringValue JobTitle { get; set; }
        public StringValue LastName { get; set; }
        public StringValue MiddleName { get; set; }
        public StringValue Phone1 { get; set; }
        public StringValue Phone1Type { get; set; }
        public StringValue Phone2 { get; set; }
        public StringValue Phone2Type { get; set; }
        public StringValue Title { get; set; }
        public StringValue WebSite { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
