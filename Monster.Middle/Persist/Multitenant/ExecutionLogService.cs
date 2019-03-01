using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Persist.Multitenant
{
    public class ExecutionLogService
    {
        private readonly PersistContext _dataContext;
        private readonly IPushLogger _logger;

        public ExecutionLogService(PersistContext dataContext, IPushLogger logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        private MonsterDataContext Entities => _dataContext.Entities;

        public void InsertExecutionLog(string content)
        {
            var logEntry = new UsrExecutionLog();
            logEntry.LogContent = content;
            logEntry.DateCreated = DateTime.UtcNow;
            Entities.UsrExecutionLogs.Add(logEntry);
            Entities.SaveChanges();
        }

        public List<UsrExecutionLog> RetrieveExecutionLogs(int take = 1000)
        {
            return Entities
                .UsrExecutionLogs
                .OrderByDescending(x => x.DateCreated)
                .Take(take)
                .ToList();
        }

        public bool RunTransaction(Action action, string type, string identifier)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                InsertExecutionLog($"Failed to execute {type} for {identifier}");
                return false;
            }
        }

    }
}
