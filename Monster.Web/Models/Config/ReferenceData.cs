using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Web.Models.Config
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

        public List<RefDataValidation> Validation {
            get
            {
                var output = new List<RefDataValidation>();

                if (TimeZones.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Time Zones are empty"));
                }

                if (ItemClasses.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Item Classes are empty"));
                }

                if (ItemClasses.All(x => x.PostingClass.IsNullOrEmpty()))
                {
                    output.Add(RefDataValidation.Invalid("None of the Item Classes contain valid Posting Class"));
                }
                
                if (PaymentMethods.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Payment Methods are empty"));
                }

                if (PaymentMethods.All(x => x.CashAccounts == null || x.CashAccounts.Count == 0))
                {
                    output.Add(RefDataValidation.Invalid("Non of the Payment Methods contain valid Allowed Cash Accounts"));
                }

                if (TaxCategories.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Tax Categories are empty"));
                }

                if (TaxIds.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Tax Ids are empty"));
                }

                if (TaxZones.Count == 0)
                {
                    output.Add(RefDataValidation.Invalid("Tax Zones are empty"));
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

        public static RefDataValidation Ok(string message)
        {
            return new RefDataValidation
            {
                IsValid = true,
                Message = message
            };
        }
    }
}
