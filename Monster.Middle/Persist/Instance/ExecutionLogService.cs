using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Services
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
            var logEntry = new ExecutionLog();
            logEntry.LogContent = content;
            logEntry.DateCreated = DateTime.UtcNow;
            Entities.ExecutionLogs.Add(logEntry);
            Entities.SaveChanges();
        }

        public List<ExecutionLog> RetrieveExecutionLogs(int take = 1000)
        {
            return Entities
                .ExecutionLogs
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
