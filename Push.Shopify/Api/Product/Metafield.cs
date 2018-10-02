using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Product
{
    public class Metafield
    {
        public long id { get; set; }
        public string @namespace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public long owner_id { get; set; }
        

        public override string ToString()
        {
            return $"id: {@id} - " + 
                   $"namespace:{@namespace } - " +
                   $"key:{@namespace} - " +
                   $"value_type:{value_type} - " +
                   $"value:{value}";
        }
    }

    public class MetafieldSingle
    {
        public Metafield metafield { get; set; }

        public static MetafieldSingle 
                MakeForUpdate(long id, string value_type, string value)
        {
            var update = new MetafieldSingle
            {
                metafield = new Metafield
                {
                    id = id,
                    value_type = value_type,
                    value = value,
                }
            };
            return update;
        }

        public static MetafieldSingle 
                MakeForInsert(string @namespace, string key, string value_type, string value)
        {
            var update = new MetafieldSingle
            {
                metafield = new Metafield
                {
                    @namespace = @namespace,
                    key = key,
                    value_type = value_type,
                    value = value,
                }
            };
            return update;
        }

    }

    public class MetafieldList
    {
        public List<Metafield> metafields { get; set; }
    }

    public static class MetafieldExtensions
    {
        public static Metafield Find(
                this List<Metafield> metafields, 
                string namescape, 
                string key)
        {
            return metafields
                .FirstOrDefault(
                    x => x.@namespace == namescape && x.key == key);
        }
    }
}
