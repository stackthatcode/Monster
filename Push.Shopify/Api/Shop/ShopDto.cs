using System;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Shop
{
    public class Shop
    {
        public long id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string domain { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string address1 { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string source { get; set; }
        public string phone { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string primary_locale { get; set; }
        public string address2 { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string currency { get; set; }
        public string customer_email { get; set; }
        public string timezone { get; set; }
        public string iana_timezone { get; set; }
        public string shop_owner { get; set; }
        public string money_format { get; set; }
        public string money_with_currency_format { get; set; }
        public string weight_unit { get; set; }
        public string province_code { get; set; }
        public string taxes_included { get; set; }
        public string tax_shipping { get; set; }
        public bool county_taxes { get; set; }
        public string plan_display_name { get; set; }
        public string plan_name { get; set; }
        public bool has_discounts { get; set; }
        public bool has_gift_cards { get; set; }
        public string myshopify_domain { get; set; }
        public string google_apps_domain { get; set; }
        public string google_apps_login_enabled { get; set; }
        public string money_in_emails_format { get; set; }
        public string money_with_currency_in_emails_format { get; set; }
        public bool eligible_for_payments { get; set; }
        public bool requires_extra_payments_agreement { get; set; }
        public bool password_enabled { get; set; }
        public bool has_storefront { get; set; }
        public bool eligible_for_card_reader_giveaway { get; set; }
        public bool finances { get; set; }
        public int primary_location_id { get; set; }
        public bool checkout_api_supported { get; set; }
        public bool multi_location_enabled { get; set; }
        public bool setup_required { get; set; }
        public bool force_ssl { get; set; }
        public bool pre_launch_enabled { get; set; }
    }

    public class ShopParent
    {
        public Shop shop { get; set; }
    }
}
