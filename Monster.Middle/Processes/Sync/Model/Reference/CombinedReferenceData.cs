using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Reference
{
    public class CombinedReferenceData
    {
        public List<AcumaticaTimeZone> TimeZones { get; set; }

        public List<ItemClassModel> ItemClasses { get; set; }
        public List<PaymentGateway> PaymentGateways { get; set; }
        public List<PaymentMethodModel> PaymentMethods { get; set; }

        public List<string> TaxIds { get; set; }
        public List<string> TaxCategories { get; set; }
        public List<string> TaxZones { get; set; }
        public List<string> CustomerClasses { get; set; }


        public CombinedReferenceData()
        {
            TimeZones = new List<AcumaticaTimeZone>();
            ItemClasses = new List<ItemClassModel>();
            PaymentMethods = new List<PaymentMethodModel>();

            TaxIds = new List<string>();
            TaxCategories = new List<string>();
            TaxZones = new List<string>();

            CustomerClasses = new List<string>();
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

                if (PaymentGateways.Count == 0)
                {
                    output.Add("Payment Gateways are empty");
                }

                if (PaymentMethods.Count == 0)
                {
                    output.Add("Payment Methods are empty");
                }

                if (PaymentMethods.All(x => x.CashAccounts == null || x.CashAccounts.Count == 0))
                {
                    output.Add("None of the Payment Methods contain valid Allowed Cash Accounts");
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

                if (CustomerClasses.Count == 0)
                {
                    output.Add("Customer Classes are empty");
                }

                return output;
            }
        }

    }
}

