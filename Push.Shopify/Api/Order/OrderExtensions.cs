using Push.Foundation.Utilities.Json;


namespace Push.Shopify.Api.Order
{
    public static class OrderExtensions
    {
        public static OrderList DeserializeToOrderList(this string input)
        {
            var output = input.DeserializeFromJson<OrderList>();
            foreach (var order in output.orders)
            {
                order.Initialize();
            }

            return output;
        }

        public static Order DeserializeToOrder(this string input)
        {
            var output = input.DeserializeFromJson<Order>();
            output.Initialize();
            return output;
        }

        public static OrderParent DeserializeToOrderParent(this string input)
        {
            var output = input.DeserializeFromJson<OrderParent>();
            output.order?.Initialize();
            return output;
        }
    } 
}
