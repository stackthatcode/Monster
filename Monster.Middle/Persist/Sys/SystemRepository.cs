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

        public IList<Installation> RetrieveTenants()
        {
            var sql = @"SELECT * FROM usrInstallation;";
            return _connection.Query<Installation>(sql).ToList();
        }

        public Installation RetrieveInstallation(Guid installationId)
        {
            var sql = 
                @"SELECT * FROM usrInstallation 
                WHERE InstallationId = @installationId";

            return _connection
                    .QueryFirstOrDefault<Installation>(
                        sql, new { installationId = installationId });
        }

        public Installation InsertTenant(
                        string connectionString, long companyId)
        {
            var tenant = new Installation()
            {
                InstallationId = Guid.NewGuid(),
                ConnectionString = connectionString,
                CompanyId = companyId
            };

            var sql =
                @"INSERT INTO usrInstallation VALUES ( " +
                @"@InstallationId, @ConnectionString, @CompanyId )";

            _connection.Execute(sql, tenant);
            return tenant;
        }
    }
}

