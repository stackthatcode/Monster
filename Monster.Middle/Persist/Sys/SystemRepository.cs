using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Monster.Middle.Persist.Sys
{
    public class SystemRepository
    {
        private readonly IDbConnection _connection;

        public SystemRepository(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public IList<Tenant> RetrieveTenants()
        {
            var sql = @"SELECT * FROM usrTenant;";
            return _connection.Query<Tenant>(sql).ToList();
        }

        public Tenant RetrieveTenant(Guid tenantID)
        {
            var sql = @"SELECT * FROM usrTenant WHERE TenantId = @tenantID";
            return _connection.QueryFirstOrDefault<Tenant>(sql, new { tenantID });
        }

        public Tenant InsertTenant(
                        string connectionString, long companyId)
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

