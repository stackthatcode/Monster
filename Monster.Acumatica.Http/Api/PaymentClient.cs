using Monster.Acumatica.Http;

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

        public string RetrievePayment(string referenceNbr, string paymentType, string expand)
        {
            var path = $"Payment/{paymentType}/{referenceNbr}?$expand={expand}";
            var response = _httpContext.Get(path);
            return response.Body;
        }
    }
}
