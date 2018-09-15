using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.EF
{
    public class Repository
    {
        private readonly MonsterDataContext _dataContext;

        public Repository(MonsterDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
    }
}

