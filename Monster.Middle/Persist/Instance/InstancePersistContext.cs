using System;

namespace Monster.Middle.Persist.Instance
{
    public class InstancePersistContext : IDisposable
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

            // CRITICAL => Lifetime Scoped PersistContext always explicitly controls
            // .. the life cycle of its (EF) Database Context
            ///
            Entities = new MonsterDataContext(ConnectionString);
        }

        public void Dispose()
        {
            Entities?.Dispose();
        }
    }
}
