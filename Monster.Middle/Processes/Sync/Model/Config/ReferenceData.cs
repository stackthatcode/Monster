using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Config
{
    public class ReferenceData
    {
        public List<TimeZone> TimeZones { get; set; }
        public List<ItemClassModel> ItemClasses { get; set; }
        public List<PaymentMethodModel> PaymentMethods { get; set; }
        public List<string> TaxIds { get; set; }
        public List<string> TaxCategories { get; set; }
        public List<string> TaxZones { get; set; }

        public ReferenceData()
        {
            TimeZones = new List<TimeZone>();
            ItemClasses = new List<ItemClassModel>();
            PaymentMethods = new List<PaymentMethodModel>();
            TaxIds = new List<string>();
            TaxCategories = new List<string>();
            TaxZones = new List<string>();
        }

        public bool IsValid => Validation.Count == 0;

        public List<string> Validation {
            get
            {
                var output = new List<string>();

                if (TimeZones.Count == 0)
                {
                    output.Add("Time Zones are empty");
                }

                if (ItemClasses.Count == 0)
                {
                    output.Add("Item Classes are empty");
                }

                if (ItemClasses.All(x => x.PostingClass.IsNullOrEmpty()))
                {
                    output.Add("None of the Item Classes contain valid Posting Class");
                }
                
                if (PaymentMethods.Count == 0)
                {
                    output.Add("Payment Methods are empty");
                }

                if (PaymentMethods.All(x => x.CashAccounts == null || x.CashAccounts.Count == 0))
                {
                    output.Add("Non of the Payment Methods contain valid Allowed Cash Accounts");
                }

                if (TaxCategories.Count == 0)
                {
                    output.Add("Tax Categories are empty");
                }

                if (TaxIds.Count == 0)
                {
                    output.Add("Tax Ids are empty");
                }

                if (TaxZones.Count == 0)
                {
                    output.Add("Tax Zones are empty");
                }

                return output;
            }
        }
    }

    public class RefDataValidation
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public static RefDataValidation Invalid(string message)
        {
            return new RefDataValidation
            {
                IsValid = false,
                Message = message
            };
        }        
    }
}
