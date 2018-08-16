using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Product
{

    public class Product
    {
        public long id { get; set; }
        public string title { get; set; }
        public string body_html { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public DateTime created_at { get; set; }
        public string handle { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime published_at { get; set; }
        public string template_suffix { get; set; }
        public string tags { get; set; }
        public string published_scope { get; set; }
        public List<Variant> variants { get; set; }
        public List<Option> options { get; set; }
        public List<Image> images { get; set; }
        public Image image { get; set; }


        public List<long> InventoryItemIds 
                => variants
                    .Select(x => x.inventory_item_id)
                    .ToList();
    }

    public class ProductParent
    {
        public Product product { get; set; }

        public void Initialize()
        {
            product.variants.ForEach(x => x.Parent = product);
        }
    }

    public class ProductList
    {
        public List<Product> products { get; set; }
    }
}
