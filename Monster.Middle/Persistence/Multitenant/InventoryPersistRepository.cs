using System.Collections.Generic;

namespace Monster.Middle.Persistence.Multitenant
{
    public class InventoryPersistRepository
    {
        private readonly MonsterDataContext _dataContext;

        public InventoryPersistRepository(MonsterDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void InsertLocation(UsrShopifyLocation location)
        {
            _dataContext.UsrShopifyLocations.Add(location);
            _dataContext.SaveChanges();
        }

        public IList<UsrShopifyLocation> RetreiveLocations()
        {
            return _dataContext.UsrShopifyLocations.ToList();
        }
    }
}
