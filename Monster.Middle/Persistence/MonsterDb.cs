﻿using System;

namespace Monster.Middle.Persistence
{

    #region Unit of work

    public interface IMonsterDataContext : System.IDisposable
    {
        System.Data.Entity.DbSet<UsrPayoutPreference> UsrPayoutPreferences { get; set; } // usrPayoutPreferences
        System.Data.Entity.DbSet<UsrShopifyLocation> UsrShopifyLocations { get; set; } // usrShopifyLocation
        System.Data.Entity.DbSet<UsrShopifyPayout> UsrShopifyPayouts { get; set; } // usrShopifyPayout
        System.Data.Entity.DbSet<UsrShopifyPayoutTransaction> UsrShopifyPayoutTransactions { get; set; } // usrShopifyPayoutTransaction
        System.Data.Entity.DbSet<UsrShopifyProduct> UsrShopifyProducts { get; set; } // usrShopifyProduct
        System.Data.Entity.DbSet<UsrShopifyVariant> UsrShopifyVariants { get; set; } // usrShopifyVariant
        System.Data.Entity.DbSet<UsrTenant> UsrTenants { get; set; } // usrTenant
        System.Data.Entity.DbSet<UsrTenantContext> UsrTenantContexts { get; set; } // usrTenantContext

        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync();
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken);
        System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker { get; }
        System.Data.Entity.Infrastructure.DbContextConfiguration Configuration { get; }
        System.Data.Entity.Database Database { get; }
        System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity);
        System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors();
        System.Data.Entity.DbSet Set(System.Type entityType);
        System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class;
        string ToString();
    }

    #endregion

    #region Database context

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class MonsterDataContext : System.Data.Entity.DbContext, IMonsterDataContext
    {
        public System.Data.Entity.DbSet<UsrPayoutPreference> UsrPayoutPreferences { get; set; } // usrPayoutPreferences
        public System.Data.Entity.DbSet<UsrShopifyLocation> UsrShopifyLocations { get; set; } // usrShopifyLocation
        public System.Data.Entity.DbSet<UsrShopifyPayout> UsrShopifyPayouts { get; set; } // usrShopifyPayout
        public System.Data.Entity.DbSet<UsrShopifyPayoutTransaction> UsrShopifyPayoutTransactions { get; set; } // usrShopifyPayoutTransaction
        public System.Data.Entity.DbSet<UsrShopifyProduct> UsrShopifyProducts { get; set; } // usrShopifyProduct
        public System.Data.Entity.DbSet<UsrShopifyVariant> UsrShopifyVariants { get; set; } // usrShopifyVariant
        public System.Data.Entity.DbSet<UsrTenant> UsrTenants { get; set; } // usrTenant
        public System.Data.Entity.DbSet<UsrTenantContext> UsrTenantContexts { get; set; } // usrTenantContext

        static MonsterDataContext()
        {
            System.Data.Entity.Database.SetInitializer<MonsterDataContext>(null);
        }

        public MonsterDataContext()
            : base("Name=DefaultConnectionString")
        {
        }

        public MonsterDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public MonsterDataContext(string connectionString, System.Data.Entity.Infrastructure.DbCompiledModel model)
            : base(connectionString, model)
        {
        }

        public MonsterDataContext(System.Data.Common.DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public MonsterDataContext(System.Data.Common.DbConnection existingConnection, System.Data.Entity.Infrastructure.DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public bool IsSqlParameterNull(System.Data.SqlClient.SqlParameter param)
        {
            var sqlValue = param.SqlValue;
            var nullableValue = sqlValue as System.Data.SqlTypes.INullable;
            if (nullableValue != null)
                return nullableValue.IsNull;
            return (sqlValue == null || sqlValue == System.DBNull.Value);
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new UsrPayoutPreferenceConfiguration());
            modelBuilder.Configurations.Add(new UsrShopifyLocationConfiguration());
            modelBuilder.Configurations.Add(new UsrShopifyPayoutConfiguration());
            modelBuilder.Configurations.Add(new UsrShopifyPayoutTransactionConfiguration());
            modelBuilder.Configurations.Add(new UsrShopifyProductConfiguration());
            modelBuilder.Configurations.Add(new UsrShopifyVariantConfiguration());
            modelBuilder.Configurations.Add(new UsrTenantConfiguration());
            modelBuilder.Configurations.Add(new UsrTenantContextConfiguration());
        }

        public static System.Data.Entity.DbModelBuilder CreateModel(System.Data.Entity.DbModelBuilder modelBuilder, string schema)
        {
            modelBuilder.Configurations.Add(new UsrPayoutPreferenceConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrShopifyLocationConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrShopifyPayoutConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrShopifyPayoutTransactionConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrShopifyProductConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrShopifyVariantConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrTenantConfiguration(schema));
            modelBuilder.Configurations.Add(new UsrTenantContextConfiguration(schema));
            return modelBuilder;
        }
    }
    #endregion

    #region Database context factory

    public class MonsterDataContextFactory : System.Data.Entity.Infrastructure.IDbContextFactory<MonsterDataContext>
    {
        public MonsterDataContext Create()
        {
            return new MonsterDataContext();
        }
    }

    #endregion

    #region Fake Database context

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class FakeMonsterDataContext : IMonsterDataContext
    {
        public System.Data.Entity.DbSet<UsrPayoutPreference> UsrPayoutPreferences { get; set; }
        public System.Data.Entity.DbSet<UsrShopifyLocation> UsrShopifyLocations { get; set; }
        public System.Data.Entity.DbSet<UsrShopifyPayout> UsrShopifyPayouts { get; set; }
        public System.Data.Entity.DbSet<UsrShopifyPayoutTransaction> UsrShopifyPayoutTransactions { get; set; }
        public System.Data.Entity.DbSet<UsrShopifyProduct> UsrShopifyProducts { get; set; }
        public System.Data.Entity.DbSet<UsrShopifyVariant> UsrShopifyVariants { get; set; }
        public System.Data.Entity.DbSet<UsrTenant> UsrTenants { get; set; }
        public System.Data.Entity.DbSet<UsrTenantContext> UsrTenantContexts { get; set; }

        public FakeMonsterDataContext()
        {
            _changeTracker = null;
            _configuration = null;
            _database = null;

            UsrPayoutPreferences = new FakeDbSet<UsrPayoutPreference>("Id");
            UsrShopifyLocations = new FakeDbSet<UsrShopifyLocation>("ShopifyLocationId");
            UsrShopifyPayouts = new FakeDbSet<UsrShopifyPayout>("ShopifyPayoutId");
            UsrShopifyPayoutTransactions = new FakeDbSet<UsrShopifyPayoutTransaction>("ShopifyPayoutId", "ShopifyPayoutTransId");
            UsrShopifyProducts = new FakeDbSet<UsrShopifyProduct>("ShopifyProductId");
            UsrShopifyVariants = new FakeDbSet<UsrShopifyVariant>("ShopifyVariantId");
            UsrTenants = new FakeDbSet<UsrTenant>("TenantId");
            UsrTenantContexts = new FakeDbSet<UsrTenantContext>("CompanyId");
        }

        public int SaveChangesCount { get; private set; }
        public int SaveChanges()
        {
            ++SaveChangesCount;
            return 1;
        }

        public System.Threading.Tasks.Task<int> SaveChangesAsync()
        {
            ++SaveChangesCount;
            return System.Threading.Tasks.Task<int>.Factory.StartNew(() => 1);
        }

        public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
        {
            ++SaveChangesCount;
            return System.Threading.Tasks.Task<int>.Factory.StartNew(() => 1, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private System.Data.Entity.Infrastructure.DbChangeTracker _changeTracker;
        public System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker { get { return _changeTracker; } }
        private System.Data.Entity.Infrastructure.DbContextConfiguration _configuration;
        public System.Data.Entity.Infrastructure.DbContextConfiguration Configuration { get { return _configuration; } }
        private System.Data.Entity.Database _database;
        public System.Data.Entity.Database Database { get { return _database; } }
        public System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new System.NotImplementedException();
        }
        public System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity)
        {
            throw new System.NotImplementedException();
        }
        public System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors()
        {
            throw new System.NotImplementedException();
        }
        public System.Data.Entity.DbSet Set(System.Type entityType)
        {
            throw new System.NotImplementedException();
        }
        public System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            throw new System.NotImplementedException();
        }
        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

    }

    // ************************************************************************
    // Fake DbSet
    // Implementing Find:
    //      The Find method is difficult to implement in a generic fashion. If
    //      you need to test code that makes use of the Find method it is
    //      easiest to create a test DbSet for each of the entity types that
    //      need to support find. You can then write logic to find that
    //      particular type of entity, as shown below:
    //      public class FakeBlogDbSet : FakeDbSet<Blog>
    //      {
    //          public override Blog Find(params object[] keyValues)
    //          {
    //              var id = (int) keyValues.Single();
    //              return this.SingleOrDefault(b => b.BlogId == id);
    //          }
    //      }
    //      Read more about it here: https://msdn.microsoft.com/en-us/data/dn314431.aspx
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class FakeDbSet<TEntity> : System.Data.Entity.DbSet<TEntity>, IQueryable, System.Collections.Generic.IEnumerable<TEntity>, System.Data.Entity.Infrastructure.IDbAsyncEnumerable<TEntity> where TEntity : class
    {
        private readonly System.Reflection.PropertyInfo[] _primaryKeys;
        private readonly System.Collections.ObjectModel.ObservableCollection<TEntity> _data;
        private readonly IQueryable _query;

        public FakeDbSet()
        {
            _data = new System.Collections.ObjectModel.ObservableCollection<TEntity>();
            _query = _data.AsQueryable();
        }

        public FakeDbSet(params string[] primaryKeys)
        {
            _primaryKeys = typeof(TEntity).GetProperties().Where(x => primaryKeys.Contains(x.Name)).ToArray();
            _data = new System.Collections.ObjectModel.ObservableCollection<TEntity>();
            _query = _data.AsQueryable();
        }

        public override TEntity Find(params object[] keyValues)
        {
            if (_primaryKeys == null)
                throw new System.ArgumentException("No primary keys defined");
            if (keyValues.Length != _primaryKeys.Length)
                throw new System.ArgumentException("Incorrect number of keys passed to Find method");

            var keyQuery = this.AsQueryable();
            keyQuery = keyValues
                .Select((t, i) => i)
                .Aggregate(keyQuery,
                    (current, x) =>
                        current.Where(entity => _primaryKeys[x].GetValue(entity, null).Equals(keyValues[x])));

            return keyQuery.SingleOrDefault();
        }

        public override System.Threading.Tasks.Task<TEntity> FindAsync(System.Threading.CancellationToken cancellationToken, params object[] keyValues)
        {
            return System.Threading.Tasks.Task<TEntity>.Factory.StartNew(() => Find(keyValues), cancellationToken);
        }

        public override System.Threading.Tasks.Task<TEntity> FindAsync(params object[] keyValues)
        {
            return System.Threading.Tasks.Task<TEntity>.Factory.StartNew(() => Find(keyValues));
        }

        public override System.Collections.Generic.IEnumerable<TEntity> AddRange(System.Collections.Generic.IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new System.ArgumentNullException("entities");
            var items = entities.ToList();
            foreach (var entity in items)
            {
                _data.Add(entity);
            }
            return items;
        }

        public override TEntity Add(TEntity item)
        {
            if (item == null) throw new System.ArgumentNullException("item");
            _data.Add(item);
            return item;
        }

        public override System.Collections.Generic.IEnumerable<TEntity> RemoveRange(System.Collections.Generic.IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new System.ArgumentNullException("entities");
            var items = entities.ToList();
            foreach (var entity in items)
            {
                _data.Remove(entity);
            }
            return items;
        }

        public override TEntity Remove(TEntity item)
        {
            if (item == null) throw new System.ArgumentNullException("item");
            _data.Remove(item);
            return item;
        }

        public override TEntity Attach(TEntity item)
        {
            if (item == null) throw new System.ArgumentNullException("item");
            _data.Add(item);
            return item;
        }

        public override TEntity Create()
        {
            return System.Activator.CreateInstance<TEntity>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return System.Activator.CreateInstance<TDerivedEntity>();
        }

        public override System.Collections.ObjectModel.ObservableCollection<TEntity> Local
        {
            get { return _data; }
        }

        System.Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new FakeDbAsyncQueryProvider<TEntity>(_query.Provider); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        System.Collections.Generic.IEnumerator<TEntity> System.Collections.Generic.IEnumerable<TEntity>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        System.Data.Entity.Infrastructure.IDbAsyncEnumerator<TEntity> System.Data.Entity.Infrastructure.IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new FakeDbAsyncEnumerator<TEntity>(_data.GetEnumerator());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class FakeDbAsyncQueryProvider<TEntity> : System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public FakeDbAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            var m = expression as System.Linq.Expressions.MethodCallExpression;
            if (m != null)
            {
                var resultType = m.Method.ReturnType; // it shoud be IQueryable<T>
                var tElement = resultType.GetGenericArguments()[0];
                var queryType = typeof(FakeDbAsyncEnumerable<>).MakeGenericType(tElement);
                return (IQueryable) System.Activator.CreateInstance(queryType, expression);
            }
            return new FakeDbAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            var queryType = typeof(FakeDbAsyncEnumerable<>).MakeGenericType(typeof(TElement));
            return (IQueryable<TElement>)System.Activator.CreateInstance(queryType, expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public System.Threading.Tasks.Task<object> ExecuteAsync(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.FromResult(Execute(expression));
        }

        public System.Threading.Tasks.Task<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.FromResult(Execute<TResult>(expression));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class FakeDbAsyncEnumerable<T> : EnumerableQuery<T>, System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public FakeDbAsyncEnumerable(System.Collections.Generic.IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public FakeDbAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new FakeDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        System.Data.Entity.Infrastructure.IDbAsyncEnumerator System.Data.Entity.Infrastructure.IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new FakeDbAsyncQueryProvider<T>(this); }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class FakeDbAsyncEnumerator<T> : System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T>
    {
        private readonly System.Collections.Generic.IEnumerator<T> _inner;

        public FakeDbAsyncEnumerator(System.Collections.Generic.IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public System.Threading.Tasks.Task<bool> MoveNextAsync(System.Threading.CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.FromResult(_inner.MoveNext());
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        object System.Data.Entity.Infrastructure.IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }
    }

    #endregion

    #region POCO classes

    // usrPayoutPreferences
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrPayoutPreference
    {
        public int Id { get; set; } // Id (Primary key)
        public string AcumaticaCashAccount { get; set; } // AcumaticaCashAccount (length: 50)
    }

    // The table 'usrPreferences' is not usable by entity framework because it
    // does not have a primary key. It is listed here for completeness.
    // usrPreferences
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrPreference
    {
        public string DefaultItemClass { get; set; } // DefaultItemClass (length: 50)
    }

    // usrShopifyLocation
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyLocation
    {
        public long ShopifyLocationId { get; set; } // ShopifyLocationId (Primary key)
        public string ShopifyJson { get; set; } // ShopifyJson
        public System.DateTime? DateCreated { get; set; } // DateCreated
        public System.DateTime? LastUpdated { get; set; } // LastUpdated
    }

    // usrShopifyPayout
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyPayout
    {
        public long ShopifyPayoutId { get; set; } // ShopifyPayoutId (Primary key)
        public string ShopifyLastStatus { get; set; } // ShopifyLastStatus (length: 50)
        public string Json { get; set; } // Json (length: 2147483647)
        public bool AllShopifyTransDownloaded { get; set; } // AllShopifyTransDownloaded
        public System.DateTime? CreatedDate { get; set; } // CreatedDate
        public System.DateTime? UpdatedDate { get; set; } // UpdatedDate
        public string AcumaticaCashAccount { get; set; } // AcumaticaCashAccount (length: 50)
        public string AcumaticaRefNumber { get; set; } // AcumaticaRefNumber (length: 50)
        public System.DateTime? AcumaticaImportDate { get; set; } // AcumaticaImportDate

        // Reverse navigation

        /// <summary>
        /// Child UsrShopifyPayoutTransactions where [usrShopifyPayoutTransaction].[ShopifyPayoutId] point to this entity (FK_usrShopifyPayoutTransaction_usrShopifyPayout)
        /// </summary>
        public virtual System.Collections.Generic.ICollection<UsrShopifyPayoutTransaction> UsrShopifyPayoutTransactions { get; set; } // usrShopifyPayoutTransaction.FK_usrShopifyPayoutTransaction_usrShopifyPayout

        public UsrShopifyPayout()
        {
            UsrShopifyPayoutTransactions = new System.Collections.Generic.List<UsrShopifyPayoutTransaction>();
        }
    }

    // usrShopifyPayoutTransaction
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyPayoutTransaction
    {
        public long ShopifyPayoutId { get; set; } // ShopifyPayoutId (Primary key)
        public long ShopifyPayoutTransId { get; set; } // ShopifyPayoutTransId (Primary key)
        public string Type { get; set; } // Type (length: 50)
        public string Json { get; set; } // Json (length: 2147483647)
        public System.DateTime? CreatedDate { get; set; } // CreatedDate
        public System.DateTime? AcumaticaImportDate { get; set; } // AcumaticaImportDate
        public string AcumaticaExtRefNbr { get; set; } // AcumaticaExtRefNbr (length: 50)

        // Foreign keys

        /// <summary>
        /// Parent UsrShopifyPayout pointed by [usrShopifyPayoutTransaction].([ShopifyPayoutId]) (FK_usrShopifyPayoutTransaction_usrShopifyPayout)
        /// </summary>
        public virtual UsrShopifyPayout UsrShopifyPayout { get; set; } // FK_usrShopifyPayoutTransaction_usrShopifyPayout
    }

    // usrShopifyProduct
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyProduct
    {
        public long ShopifyProductId { get; set; } // ShopifyProductId (Primary key)
        public string ShopifyJson { get; set; } // ShopifyJson
        public System.DateTime? DateCreated { get; set; } // DateCreated
        public System.DateTime? LastUpdated { get; set; } // LastUpdated

        // Reverse navigation

        /// <summary>
        /// Child UsrShopifyVariants where [usrShopifyVariant].[ShopifyProductId] point to this entity (FK_usrShopifyVariant_usrShopifyProduct)
        /// </summary>
        public virtual System.Collections.Generic.ICollection<UsrShopifyVariant> UsrShopifyVariants { get; set; } // usrShopifyVariant.FK_usrShopifyVariant_usrShopifyProduct

        public UsrShopifyProduct()
        {
            UsrShopifyVariants = new System.Collections.Generic.List<UsrShopifyVariant>();
        }
    }

    // usrShopifyVariant
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyVariant
    {
        public long ShopifyVariantId { get; set; } // ShopifyVariantId (Primary key)
        public long? ShopifyProductId { get; set; } // ShopifyProductId
        public string ShopifyJson { get; set; } // ShopifyJson
        public string ShopifySku { get; set; } // ShopifySku (length: 100)
        public System.DateTime? DateCreated { get; set; } // DateCreated
        public System.DateTime? LastUpdated { get; set; } // LastUpdated

        // Foreign keys

        /// <summary>
        /// Parent UsrShopifyProduct pointed by [usrShopifyVariant].([ShopifyProductId]) (FK_usrShopifyVariant_usrShopifyProduct)
        /// </summary>
        public virtual UsrShopifyProduct UsrShopifyProduct { get; set; } // FK_usrShopifyVariant_usrShopifyProduct
    }

    // usrTenant
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrTenant
    {
        public System.Guid TenantId { get; set; } // TenantId (Primary key)
        public string ConnectionString { get; set; } // ConnectionString (length: 500)
        public long? CompanyId { get; set; } // CompanyID
    }

    // usrTenantContext
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrTenantContext
    {
        public long CompanyId { get; set; } // CompanyID (Primary key)
        public string ShopifyDomain { get; set; } // ShopifyDomain (length: 500)
        public string ShopifyApiKey { get; set; } // ShopifyApiKey (length: 500)
        public string ShopifyApiPassword { get; set; } // ShopifyApiPassword (length: 500)
        public string ShopifyApiSecret { get; set; } // ShopifyApiSecret (length: 500)
        public string AcumaticaInstanceUrl { get; set; } // AcumaticaInstanceUrl (length: 500)
        public string AcumaticaBranch { get; set; } // AcumaticaBranch (length: 500)
        public string AcumaticaCompanyName { get; set; } // AcumaticaCompanyName (length: 500)
        public string AcumaticaUsername { get; set; } // AcumaticaUsername (length: 500)
        public string AcumaticaPassword { get; set; } // AcumaticaPassword (length: 500)
    }

    #endregion

    #region POCO Configuration

    // usrPayoutPreferences
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrPayoutPreferenceConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrPayoutPreference>
    {
        public UsrPayoutPreferenceConfiguration()
            : this("dbo")
        {
        }

        public UsrPayoutPreferenceConfiguration(string schema)
        {
            ToTable("usrPayoutPreferences", schema);
            HasKey(x => x.Id);

            Property(x => x.Id).HasColumnName(@"Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            Property(x => x.AcumaticaCashAccount).HasColumnName(@"AcumaticaCashAccount").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
        }
    }

    // usrShopifyLocation
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyLocationConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrShopifyLocation>
    {
        public UsrShopifyLocationConfiguration()
            : this("dbo")
        {
        }

        public UsrShopifyLocationConfiguration(string schema)
        {
            ToTable("usrShopifyLocation", schema);
            HasKey(x => x.ShopifyLocationId);

            Property(x => x.ShopifyLocationId).HasColumnName(@"ShopifyLocationId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyJson).HasColumnName(@"ShopifyJson").HasColumnType("nvarchar(max)").IsOptional();
            Property(x => x.DateCreated).HasColumnName(@"DateCreated").HasColumnType("datetime").IsOptional();
            Property(x => x.LastUpdated).HasColumnName(@"LastUpdated").HasColumnType("datetime").IsOptional();
        }
    }

    // usrShopifyPayout
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyPayoutConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrShopifyPayout>
    {
        public UsrShopifyPayoutConfiguration()
            : this("dbo")
        {
        }

        public UsrShopifyPayoutConfiguration(string schema)
        {
            ToTable("usrShopifyPayout", schema);
            HasKey(x => x.ShopifyPayoutId);

            Property(x => x.ShopifyPayoutId).HasColumnName(@"ShopifyPayoutId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyLastStatus).HasColumnName(@"ShopifyLastStatus").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(50);
            Property(x => x.Json).HasColumnName(@"Json").HasColumnType("text").IsRequired().IsUnicode(false).HasMaxLength(2147483647);
            Property(x => x.AllShopifyTransDownloaded).HasColumnName(@"AllShopifyTransDownloaded").HasColumnType("bit").IsRequired();
            Property(x => x.CreatedDate).HasColumnName(@"CreatedDate").HasColumnType("datetime").IsOptional();
            Property(x => x.UpdatedDate).HasColumnName(@"UpdatedDate").HasColumnType("datetime").IsOptional();
            Property(x => x.AcumaticaCashAccount).HasColumnName(@"AcumaticaCashAccount").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.AcumaticaRefNumber).HasColumnName(@"AcumaticaRefNumber").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.AcumaticaImportDate).HasColumnName(@"AcumaticaImportDate").HasColumnType("datetime").IsOptional();
        }
    }

    // usrShopifyPayoutTransaction
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyPayoutTransactionConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrShopifyPayoutTransaction>
    {
        public UsrShopifyPayoutTransactionConfiguration()
            : this("dbo")
        {
        }

        public UsrShopifyPayoutTransactionConfiguration(string schema)
        {
            ToTable("usrShopifyPayoutTransaction", schema);
            HasKey(x => new { x.ShopifyPayoutId, x.ShopifyPayoutTransId });

            Property(x => x.ShopifyPayoutId).HasColumnName(@"ShopifyPayoutId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyPayoutTransId).HasColumnName(@"ShopifyPayoutTransId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.Type).HasColumnName(@"Type").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.Json).HasColumnName(@"Json").HasColumnType("text").IsOptional().IsUnicode(false).HasMaxLength(2147483647);
            Property(x => x.CreatedDate).HasColumnName(@"CreatedDate").HasColumnType("datetime").IsOptional();
            Property(x => x.AcumaticaImportDate).HasColumnName(@"AcumaticaImportDate").HasColumnType("datetime").IsOptional();
            Property(x => x.AcumaticaExtRefNbr).HasColumnName(@"AcumaticaExtRefNbr").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);

            // Foreign keys
            HasRequired(a => a.UsrShopifyPayout).WithMany(b => b.UsrShopifyPayoutTransactions).HasForeignKey(c => c.ShopifyPayoutId).WillCascadeOnDelete(false); // FK_usrShopifyPayoutTransaction_usrShopifyPayout
        }
    }

    // usrShopifyProduct
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyProductConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrShopifyProduct>
    {
        public UsrShopifyProductConfiguration()
            : this("dbo")
        {
        }

        public UsrShopifyProductConfiguration(string schema)
        {
            ToTable("usrShopifyProduct", schema);
            HasKey(x => x.ShopifyProductId);

            Property(x => x.ShopifyProductId).HasColumnName(@"ShopifyProductId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyJson).HasColumnName(@"ShopifyJson").HasColumnType("nvarchar(max)").IsOptional();
            Property(x => x.DateCreated).HasColumnName(@"DateCreated").HasColumnType("datetime").IsOptional();
            Property(x => x.LastUpdated).HasColumnName(@"LastUpdated").HasColumnType("datetime").IsOptional();
        }
    }

    // usrShopifyVariant
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrShopifyVariantConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrShopifyVariant>
    {
        public UsrShopifyVariantConfiguration()
            : this("dbo")
        {
        }

        public UsrShopifyVariantConfiguration(string schema)
        {
            ToTable("usrShopifyVariant", schema);
            HasKey(x => x.ShopifyVariantId);

            Property(x => x.ShopifyVariantId).HasColumnName(@"ShopifyVariantId").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyProductId).HasColumnName(@"ShopifyProductId").HasColumnType("bigint").IsOptional();
            Property(x => x.ShopifyJson).HasColumnName(@"ShopifyJson").HasColumnType("nvarchar(max)").IsOptional();
            Property(x => x.ShopifySku).HasColumnName(@"ShopifySku").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(100);
            Property(x => x.DateCreated).HasColumnName(@"DateCreated").HasColumnType("datetime").IsOptional();
            Property(x => x.LastUpdated).HasColumnName(@"LastUpdated").HasColumnType("datetime").IsOptional();

            // Foreign keys
            HasOptional(a => a.UsrShopifyProduct).WithMany(b => b.UsrShopifyVariants).HasForeignKey(c => c.ShopifyProductId).WillCascadeOnDelete(false); // FK_usrShopifyVariant_usrShopifyProduct
        }
    }

    // usrTenant
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrTenantConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrTenant>
    {
        public UsrTenantConfiguration()
            : this("dbo")
        {
        }

        public UsrTenantConfiguration(string schema)
        {
            ToTable("usrTenant", schema);
            HasKey(x => x.TenantId);

            Property(x => x.TenantId).HasColumnName(@"TenantId").HasColumnType("uniqueidentifier").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ConnectionString).HasColumnName(@"ConnectionString").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.CompanyId).HasColumnName(@"CompanyID").HasColumnType("bigint").IsOptional();
        }
    }

    // usrTenantContext
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.37.1.0")]
    public class UsrTenantContextConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<UsrTenantContext>
    {
        public UsrTenantContextConfiguration()
            : this("dbo")
        {
        }

        public UsrTenantContextConfiguration(string schema)
        {
            ToTable("usrTenantContext", schema);
            HasKey(x => x.CompanyId);

            Property(x => x.CompanyId).HasColumnName(@"CompanyID").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            Property(x => x.ShopifyDomain).HasColumnName(@"ShopifyDomain").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.ShopifyApiKey).HasColumnName(@"ShopifyApiKey").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.ShopifyApiPassword).HasColumnName(@"ShopifyApiPassword").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.ShopifyApiSecret).HasColumnName(@"ShopifyApiSecret").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.AcumaticaInstanceUrl).HasColumnName(@"AcumaticaInstanceUrl").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.AcumaticaBranch).HasColumnName(@"AcumaticaBranch").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.AcumaticaCompanyName).HasColumnName(@"AcumaticaCompanyName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.AcumaticaUsername).HasColumnName(@"AcumaticaUsername").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.AcumaticaPassword).HasColumnName(@"AcumaticaPassword").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
        }
    }

    #endregion

}
// </auto-generated>

