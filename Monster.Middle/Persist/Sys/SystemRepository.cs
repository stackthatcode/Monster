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

        public IList<Instance> RetrieveInstances()
        {
            var sql = @"SELECT * FROM Instance;";
            return _connection.Query<Instance>(sql).ToList();
        }

        public Instance RetrieveInstance(Guid instanceId)
        {
            var sql = @"SELECT * FROM Instance WHERE InstanceId = @instanceId";

            return _connection
                    .QueryFirstOrDefault<Instance>(
                        sql, new { instanceId = instanceId });
        }

        public Instance InsertInstance(string connectionString, string nickName = null)
        {
            var tenant = new Instance()
            {
                Id = Guid.NewGuid(),
                ConnectionString = connectionString,
                Nickname = nickName,
            };

            var sql =
                @"INSERT INTO Instance VALUES ( @Id, @ConnectionString )";

            _connection.Execute(sql, tenant);
            return tenant;
        }
    }
}

