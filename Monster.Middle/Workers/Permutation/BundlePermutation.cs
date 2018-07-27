using System.Collections.Generic;
using System.Linq;
using Monster.Middle.EF;

namespace Monster.Middle.Workers
{
    // Defined for sake of *cleanly* bounded context

    public class BundlePermutation
    {
        public List<BundlePermutationItem> Items { get; set; }        
        public int LeadProductVariantId => Items.First().ProductVariantId;
        public string LeadProductVariantName => Items.First().Name;
        public string LeadProductVariantSku => Items.First().Sku;


        public BundlePermutation(List<ProductVariant> input)
        {
            Items =
                input
                    .Select(x => new BundlePermutationItem
                    {
                        ProductVariantId = x.Id,
                        Name = x.Name,
                        Sku = x.ShopifyVariantSku,
                    })
                    .ToList();
        }

        public bool IsExcluded(List<ExclusionConstraint> constraints)
        {
            return constraints.Any(c =>
                        this.Items.Any(x => x.ProductVariantId == c.SourceProductVariantId) &&
                        this.Items.Any(x => x.ProductVariantId == c.TargetProductVariantId));
        }
        
        public string AllVariantsTitle => string.Join(" / ", Items.Select(x => x.Name));

        public string BundleVariantTitle =>
                        Items.Count > 1
                            ? string.Join(" / ", Items.Skip(1).Select(x => x.Name))
                            : LeadProductVariantName;

        public string Sku(int sequenceNumber)
        {
            return $"{LeadProductVariantSku}-BUNDLE-{sequenceNumber:0000}";
        }
    }

    public class BundlePermutationItem
    {
        public int ProductVariantId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
    }
}

