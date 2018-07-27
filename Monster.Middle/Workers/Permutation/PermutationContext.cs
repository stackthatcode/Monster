using System.Collections.Generic;
using System.Linq;
using Monster.Middle.EF;

namespace Monster.Middle.Workers
{
    public class PermutationContext
    {
        public List<ProductType> OrderedProductTypes { get; set; }
        public List<ProductVariant> AllProductVariants { get; set; }

        public PermutationContext(
                    List<ProductType> orderedProductTypes, List<ProductVariant> allProductVariants)
        {
            OrderedProductTypes = orderedProductTypes;
            AllProductVariants = allProductVariants;
        }
        
        public List<BundlePermutation> ProduceAllPermutations()
        {
            var startingProductType = OrderedProductTypes.First();
            return RecursivePermutator(new List<ProductVariant>(), startingProductType);
        }
        
        public List<BundlePermutation> RecursivePermutator(
                    List<ProductVariant> previousSequence, ProductType currentProductType)
        {
            var productVariantOfCurrentType =
                    this.AllProductVariants
                        .Where(x => x.ProductTypeId == currentProductType.Id)
                        .ToList();

            // This will account for the absence of adding a Product Type to the Bundle
            if (currentProductType != OrderedProductTypes.First())
            {
                productVariantOfCurrentType.Add(null);
            }

            var output = new List<BundlePermutation>();
            
            foreach (var productVariant in productVariantOfCurrentType)
            {
                var currentSequence = new List<ProductVariant>();
                currentSequence.AddRange(previousSequence);

                if (productVariant != null)
                {
                    currentSequence.Add(productVariant);
                }
                
                if (HasNextProductType(currentProductType))
                {
                    var nextProductType = NextProductType(currentProductType);
                    var recursiveOutput = RecursivePermutator(currentSequence, nextProductType);
                    output.AddRange(recursiveOutput);
                }
                else
                {
                    output.Add(new BundlePermutation(currentSequence));
                }
            }

            return output;
        }
        
        public ProductType NextProductType(ProductType productType)
        {
            var index = OrderedProductTypes.FindIndex(x => x.Id == productType.Id);
            var nextIndex = index + 1;

            return (nextIndex > OrderedProductTypes.Count - 1)
                ? null
                : OrderedProductTypes[nextIndex];
        }

        public bool HasNextProductType(ProductType productType)
        {
            return NextProductType(productType) != null;
        }
    }
}

