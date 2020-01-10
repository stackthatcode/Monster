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
            var sql = @"SELECT * FROM Instance ORDER BY AvailabilityOrder;";
            return _connection.Query<Instance>(sql).ToList();
        }

        public Instance RetrieveInstance(Guid instanceId)
        {
            var sql = @"SELECT * FROM Instance WHERE Id = @instanceId";

            return _connection.QueryFirstOrDefault<Instance>(sql, new { instanceId });
        }

        public int NextAvailabilityOrder()
        {
            var sql = @"SELECT MAX(AvailabilityOrder) FROM Instance";

            var maxOrder = _connection.QueryFirstOrDefault<int?>(sql);
            return (maxOrder ?? 0) + 1;
        }

        public Instance InsertInstance(string instanceDatabase, bool isEnabled)
        {
            var availabilityOrder = NextAvailabilityOrder();

            var tenant = new Instance()
            {
                Id = Guid.NewGuid(),
                AvailabilityOrder = availabilityOrder,
                InstanceDatabase = instanceDatabase,
                IsEnabled = isEnabled,
            };

            var sql = 
                @"INSERT INTO Instance (Id, AvailabilityOrder, InstanceDatabase, IsEnabled) 
	            VALUES ( @Id, @AvailabilityOrder, @InstanceDatabase, @IsEnabled )";
            _connection.Execute(sql, tenant);
            return tenant;
        }

        public void UpdateInstanceEnabled(Guid instanceId, bool enabled)
        {
            var sql = @"UPDATE Instance SET IsEnabled = @enabled WHERE Id = @instanceId";
            _connection.Execute(sql, new {instanceId, enabled});
        }

        public Instance RetrieveNextAvailableInstance()
        {
            var sql = "SELECT * FROM Instance WHERE OwnerUserID IS NULL ORDER BY AvailabilityOrder";
            return _connection.QueryFirstOrDefault<Instance>(sql);
        }

        public Instance RetrieveInstanceByDomain(string domain)
        {
            var sql = "SELECT * FROM Instance WHERE OwnerDomain = @domain";
            return _connection.Query<Instance>(sql, new { domain }).FirstOrDefault();
        }

        public List<Instance> RetrievesInstanceByUserId(string aspNetUserId)
        {
            var sql = "SELECT * FROM Instance WHERE OwnerUserId = @userId";
            return _connection.Query<Instance>(sql, new {userId = aspNetUserId}).ToList();
        }


        public void AssignInstanceToUser(Guid instanceId, string aspNetUserId, string nickname, string domain)
        {
            var sql = @"UPDATE Instance
                        SET OwnerUserId = @aspNetUserId,
                        OwnerNickName = @nickname,
                        OwnerDomain = @domain,
                        IsEnabled = 1
                        WHERE Id = @instanceId";
            
            _connection.Execute(sql, new { instanceId, aspNetUserId, nickname, domain });
        }

        public void RevokeInstance(Guid instanceId)
        {
            var sql = @"UPDATE Instance 
                        SET OwnerUserId = NULL, OwnerNickName = NULL, OwnerDomain = NULL, IsEnabled = 0
                        WHERE Id = @instanceId";

            _connection.Execute(sql, new { instanceId });
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

