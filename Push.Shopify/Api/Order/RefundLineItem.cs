﻿namespace Push.Shopify.Api.Order
{
    public class RefundLineItem
    {
        public RefundLineItem(){ }

        public Refund ParentRefund { get; set; }
        public long Id { get; set; }
        public long LineItemId { get; set; }
        public int RestockQuantity { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal SubTotal { get; set; }
    }
}
