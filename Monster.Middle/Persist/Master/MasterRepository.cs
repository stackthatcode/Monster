using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Monster.Middle.Persist.Master
{
    public class MasterRepository : IDisposable
    {
        private readonly IDbConnection _connection;

        public MasterRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public IList<Instance> RetrieveInstances()
        {
            var sql = @"SELECT * FROM Instance WHERE IsEnabled = 1;";
            return _connection.Query<Instance>(sql).ToList();
        }

        public Instance RetrieveInstance(Guid instanceId)
        {
            var sql = @"SELECT * FROM Instance WHERE Id = @instanceId";

            return _connection.QueryFirstOrDefault<Instance>(sql, new { instanceId });
        }

        public Instance InsertInstance(string connectionString, string nickName = null)
        {
            var tenant = new Instance()
            {
                Id = Guid.NewGuid(),
                ConnectionString = connectionString,
                OwnerNickname = nickName,
            };

            var sql = @"INSERT INTO Instance VALUES ( '@Id', @ConnectionString )";
            _connection.Execute(sql, tenant);
            return tenant;
        }

        public void UpdateInstanceEnabled(Guid instanceId, bool enabled)
        {
            var sql = @"UPDATE Instance SET IsEnabled = @enabled WHERE Id = @instanceId";
            _connection.Execute(sql, new {instanceId, enabled});
        }


        public Instance RetrieveInstanceByUserId(string userId)
        {
            var sql = "SELECT * FROM Instance WHERE OwnerUserId = @userId";
            return _connection.QueryFirstOrDefault<Instance>(sql, new { userId });
        }

        public Instance RetrieveNextAvailableInstance()
        {
            var sql = "SELECT * FROM Instance ORDER BY AvailabilityOrder";
            return _connection.QueryFirstOrDefault<Instance>(sql);
        }

        public void AssignInstanceToUser(Guid instanceId, string aspNetUserId, string domain)
        {
            var sql = @"UPDATE Instance
                        SET OwnerUserId = @aspNetUserId,
                        OwnerNickName = @domain
                        WHERE Id = @instanceId";
            
            _connection.Execute(sql, new { instanceId, aspNetUserId, domain });
        }


        public List<PaymentGateway> RetrievePaymentGateways()
        {
            var sql = @"SELECT * FROM PaymentGateways ORDER BY Id";
            return _connection.Query<PaymentGateway>(sql).ToList();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}

