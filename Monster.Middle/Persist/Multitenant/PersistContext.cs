using System;

namespace Monster.Middle.Persist.Multitenant
{
    public class PersistContext : IDisposable
    {
        public string ConnectionString { get; private set; }
        public long CompanyId { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();

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

        public void Dispose()
        {
            Entities?.Dispose();
        }
    }
}
