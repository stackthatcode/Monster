using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.EF
{
    public class Repository
    {
        private readonly BundleDataContext _dataContext;

        public Repository(BundleDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public List<ProductType> RetrieveProductTypesOrdered()
        {
            return _dataContext.ProductTypes.OrderBy(x => x.DisplayOrder).ToList();
        }

        public List<ProductVariant> RetrieveProductVariants()
        {
            return _dataContext.ProductVariants.ToList();
        }

        public List<ExclusionConstraint> RetrieveExclusionConstraints()
        {
            return _dataContext.ExclusionConstraints.ToList();
        }


        public void InsertBundleProduct(BundleProduct input)
        {
            _dataContext.BundleProducts.Add(input);
            _dataContext.SaveChanges();
        }
    }
}

