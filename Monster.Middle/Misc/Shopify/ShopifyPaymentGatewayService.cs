using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Master;

namespace Monster.Middle.Misc.Shopify
{
    public class ShopifyPaymentGatewayService
    {
        private static readonly List<PaymentGateway> _cachedList = new List<PaymentGateway>();

        private readonly MasterRepository _masterRepository;

        public ShopifyPaymentGatewayService(MasterRepository masterRepository)
        {
            _masterRepository = masterRepository;
        }

        private void Hydration()
        {
            lock (_cachedList)
            {
                if (!_cachedList.Any())
                {
                    var data = _masterRepository.RetrievePaymentGateways();
                    _cachedList.AddRange(data);
                }
            }
        }

        public List<PaymentGateway> Retrieve()
        {
            Hydration();
            return _cachedList;
        }

        public PaymentGateway Retrieve(string gatewayId)
        {
            Hydration();
            return _cachedList.FirstOrDefault(x => x.Id == gatewayId);
        }

        public bool Exists(string gatewayId)
        {
            return Retrieve(gatewayId) != null;
        }

        public string Name(string gatewayId)
        {
            var gateway = Retrieve().FirstOrDefault(x => x.Id == gatewayId);
            return gateway == null ? "(invalid gateway)" : gateway.Name;
        }
    }
}
