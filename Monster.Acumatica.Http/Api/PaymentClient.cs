﻿using Monster.Acumatica.Http;
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

        public string RetrievePayments()
        {
            var response = _httpContext.Get("Payment");
            return response.Body;
        }

        public string RetrievePayment(
                    string referenceNbr, string paymentType, string expand)
        {
            var path = $"Payment/{paymentType}/{referenceNbr}?$expand={expand}";
            var response = _httpContext.Get(path);
            return response.Body;
        }
    }
}
