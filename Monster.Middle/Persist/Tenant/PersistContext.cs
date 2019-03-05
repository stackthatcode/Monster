using System;

namespace Monster.Middle.Persist.Tenant
{
    public class PersistContext : IDisposable
    {
        public string ConnectionString { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();

        public MonsterDataContext Entities { get; private set; }

        public void Initialize(string connectionString)
        {
            if (Entities != null)
            {
                throw new Exception("PersistContext already initialized");
            }

            ConnectionString = connectionString;
            Entities = new MonsterDataContext(ConnectionString);
        }

        public void Dispose()
        {
            Entities?.Dispose();
        }
    }
}
