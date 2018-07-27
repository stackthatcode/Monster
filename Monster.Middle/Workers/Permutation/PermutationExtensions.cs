using System.Collections.Generic;
using System.Linq;
using Monster.Middle.EF;


namespace Monster.Middle.Workers
{
    public static class PermutationExtensions
    {

        public static List<BundlePermutation> Excluded(
                    this List<BundlePermutation> input, List<ExclusionConstraint> constraints)
        {
            return input.Where(x => x.IsExcluded(constraints)).ToList();
        }

        public static List<BundlePermutation> Included(
                    this List<BundlePermutation> input, List<ExclusionConstraint> constraints)
        {
            return input.Where(x => !x.IsExcluded(constraints)).ToList();
        }


        // For transition to another bounded context i.e. BundleProduct persistence
        public static List<BundleProduct> ToBundleProducts(
                this List<BundlePermutation> permutations)
        {
            var output = new List<BundleProduct>();
            
            var leadProductVariantsIds =
                permutations.Select(x => x.LeadProductVariantId).Distinct();

            foreach (var id in leadProductVariantsIds)
            {
                var sequenceNumber = 1;

                var permutationsForBundle =
                    permutations.Where(x => x.LeadProductVariantId == id).ToList();

                output.Add(permutationsForBundle.ToBundleProduct(ref sequenceNumber));
            }

            return output;
        }

        public static BundleProduct ToBundleProduct(
                            this List<BundlePermutation> permutations, ref int sequenceNumber)
        {
            var bundleProduct = new BundleProduct()
            {
                ProductTitle = permutations.First().LeadProductVariantName + " Bundle",
                BundleUnifiedVariants = new List<BundleUnifiedVariant>(),
            };

            foreach (var permutation in permutations)
            {
                var unifiedVariant = new BundleUnifiedVariant();
                unifiedVariant.VariantTitle = permutation.AllVariantsTitle;
                unifiedVariant.BundleVariantReferences
                    = permutation
                        .Items
                        .Select(x => new BundleVariantReference { ProductVariantId = x.ProductVariantId })
                        .ToList();

                unifiedVariant.Sku = permutation.Sku(sequenceNumber++);

                bundleProduct.BundleUnifiedVariants.Add(unifiedVariant);
            }

            return bundleProduct;
        }
    }
}

