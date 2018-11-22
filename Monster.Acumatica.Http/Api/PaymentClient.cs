using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class PaymentClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly IPushLogger _logger;

        public PaymentClient(
                AcumaticaHttpContext httpContext, IPushLogger logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        public string WritePayment(string json)
        {
            var response = _httpContext.Put("Payment", json);
            return response.Body;
        }
    }
}
