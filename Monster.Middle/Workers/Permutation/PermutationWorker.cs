using System.Linq;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Workers.Permutation
{
    public class PermutationWorker
    {
        private readonly Repository _repository;
        private readonly IPushLogger _pushLogger;

        public PermutationWorker(Repository repository, IPushLogger pushLogger)
        {
            _repository = repository;
            _pushLogger = pushLogger;
        }

        public void Do()
        {
            var productTypesOrdered = _repository.RetrieveProductTypesOrdered();
            var allProductVariants = _repository.RetrieveProductVariants();
            var constraints = _repository.RetrieveExclusionConstraints();
            
            var context = new PermutationContext(productTypesOrdered, allProductVariants);
            var allPermutations = context.ProduceAllPermutations();

            var filteredPermutations = allPermutations.Included(constraints).ToList();

            var bundleProducts = filteredPermutations.ToBundleProducts();
            bundleProducts.ForEach(x => _repository.InsertBundleProduct(x));

            foreach (var b in bundleProducts)
            {
                foreach (var bv in b.BundleUnifiedVariants)
                {
                    _pushLogger.Info($"{bv.Id},{b.ProductTitle},{bv.VariantTitle},{bv.Sku}");
                }
            }
        }
    }
}

