namespace Push.Shopify.Api.Order
{
    public class ClientDetails
    {
        public string browser_ip { get; set; }
        public string accept_language { get; set; }
        public string user_agent { get; set; }
        public object session_hash { get; set; }
        public object browser_width { get; set; }
        public object browser_height { get; set; }
    }
}