using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Monster.Middle.Persistence.System;

namespace Monster.Middle.Sql.System
{
    public class AccountRepository
    {
        private readonly IDbConnection _connection;

        public AccountRepository(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public Tenant RetrieveTenant(Guid tenantID)
        {
            var sql = @"SELECT * FROM usrTenant WHERE TenantId = @tenantID";
            return _connection.QueryFirstOrDefault<Tenant>(sql, new {tenantID});
        }

        public Tenant InsertTenant(string connectionString, long companyId)
        {
            var tenant = new Tenant()
            {
                TenantId = Guid.NewGuid(),
                ConnectionString = connectionString,
                CompanyId = companyId
            };

            var sql =
                @"INSERT INTO usrTenant VALUES ( " + 
                @"@TenantId, @ConnectionString, @CompanyId )";

            _connection.Execute(sql, tenant);
            return tenant;
        }
    }
}
