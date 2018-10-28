using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace Monster.Middle.Persist.Multitenant
{
    public class LocationRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public LocationRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void InsertShopifyLocation(UsrShopifyLocation location)
        {
            Entities.UsrShopifyLocations.Add(location);
            Entities.SaveChanges();
        }

        public IList<UsrShopifyLocation> RetreiveShopifyLocations()
        {
            return Entities
                    .UsrShopifyLocations
                    .Include(x => x.UsrAcumaticaWarehouses)
                    .ToList();
        }

        public void InsertAcumaticaWarehouse(UsrAcumaticaWarehouse warehouse)
        {
            Entities.UsrAcumaticaWarehouses.Add(warehouse);
            Entities.SaveChanges();
        }

        public IList<UsrAcumaticaWarehouse> RetreiveAcumaticaWarehouses()
        {
            return Entities
                    .UsrAcumaticaWarehouses
                    .Include(x => x.UsrShopifyLocation)
                    .ToList();
        }
        
        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
