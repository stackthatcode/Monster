using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Sql.Multitenant
{
    public class InventoryPersistRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public InventoryPersistRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void InsertLocation(UsrShopifyLocation location)
        {
            Entities.UsrShopifyLocations.Add(location);
            Entities.SaveChanges();
        }

        public IList<UsrShopifyLocation> RetreiveLocations()
        {
            return Entities.UsrShopifyLocations.ToList();
        }
    }
}
