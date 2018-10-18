namespace Monster.Middle.Persist.Multitenant
{
    class MatchRepository
    {
        private readonly PersistContext _dataContext;

        public MatchRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public MonsterDataContext Entities => _dataContext.Entities;



    }
}
