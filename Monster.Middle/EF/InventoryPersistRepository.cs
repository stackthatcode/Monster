namespace Monster.Middle.EF
{
    public class InventoryPersistRepository
    {
        private readonly MonsterDataContext _dataContext;

        public InventoryPersistRepository(MonsterDataContext dataContext)
        {
            _dataContext = dataContext;
        }


    }
}
