using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Middle.Sql;

namespace Monster.Middle.Persistence.Multitenant
{
    public class PersistContext
    {
        public string ConnectionString { get; private set; }
        public long CompanyId { get; private set; }
        public MonsterDataContext Entities { get; private set; }

        public void Initialize(string connectionString, long companyId)
        {
            if (Entities != null)
            {
                throw new Exception("PersistContext already initialized");
            }

            ConnectionString = connectionString;
            CompanyId = companyId;
            Entities = new MonsterDataContext(ConnectionString);
        }
    }
}
