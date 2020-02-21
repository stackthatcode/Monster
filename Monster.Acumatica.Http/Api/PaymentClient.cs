using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Api
{
    public class PaymentClient
    {
        private readonly AcumaticaHttpContext _httpContext;

        public PaymentClient(AcumaticaHttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public string WritePayment(string json)
        {
            var response = _httpContext.Put("Payment", json);
            return response.Body;
        }

        public string RetrievePayments()
        {
            var response = _httpContext.Get("Payment");
            return response.Body;
        }

        public Payment.Payment RetrievePayment(string referenceNbr, string paymentType)
        {
            var path = $"Payment/{paymentType}/{referenceNbr}?$expand=ApplicationHistory";
            var response = _httpContext.Get(path);
            return response.Body.DeserializeFromJson<Payment.Payment>();
        }

        public decimal RetrievePaymentBalance(string referenceNbr, string paymentType)
        {
            var acumaticaPayment = RetrievePayment(referenceNbr, paymentType);
            var acumaticaAmount = (decimal)acumaticaPayment.PaymentAmount.value;
            var acumaticaAmountPaid = (decimal)acumaticaPayment.ApplicationHistory.Sum(x => x.AmountPaid.value);
            return acumaticaAmount - acumaticaAmountPaid;
        }


        public List<Payment.Payment> RetrievePaymentByPaymentRef(string paymentRef)
        {
            var path = $"Payment?$filter=PaymentRef eq '{paymentRef}'";
            var response = _httpContext.Get(path);
            return response.Body.DeserializeFromJson<List<Payment.Payment>>();
        }


        public string ReleasePayment(string referenceNbr, string paymentType)
        {
            var payload = new
            {
                entity = new
                {
                    Type = paymentType.ToValue(),
                    ReferenceNbr = referenceNbr.ToValue(),
                }
            };

            var content = payload.SerializeToJson();
            var response = _httpContext.Post("Payment/ReleasePayment", content);
            return response.Body;
        }
    }
}
