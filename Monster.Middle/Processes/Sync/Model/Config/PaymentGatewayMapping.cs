﻿using Monster.Middle.Processes.Acumatica.Model;

namespace Monster.Middle.Processes.Sync.Model.Config
{
    public class PaymentGatewayMapping
    {
        public string ShopifyGatewayId { get; set; }
        public PaymentMethodModel PaymentMethod { get; set; }
    }
}
