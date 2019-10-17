using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Misc;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Misc.Logging
{
    public class ExecutionLogService
    {
        private readonly ProcessPersistContext _dataContext;
        private readonly IPushLogger _logger;

        public ExecutionLogService(ProcessPersistContext dataContext, IPushLogger logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        private MonsterDataContext Entities => _dataContext.Entities;

        public void InsertExecutionLog(string content, int level = LogLevel.Information)
        {
            try
            {
                var logEntry = new ExecutionLog();
                logEntry.LogContent = content;
                logEntry.LogLevel = level;
                logEntry.DateCreated = DateTime.UtcNow;
                Entities.ExecutionLogs.Add(logEntry);
                Entities.SaveChanges();
            }
            catch (Exception ex)
            {
                // Swallow the exception! - this is a logger
                //
                _logger.Info($"Failed attempt to add Execution Log ({level}) {content}");
                _logger.Error(ex);
            }
        }

        public List<ExecutionLog> RetrieveExecutionLogs(int take = 1000)
        {
            return Entities
                .ExecutionLogs
                .OrderByDescending(x => x.DateCreated)
                .Take(take)
                .ToList();
        }

        public bool ExecuteWithFailLog(Action action, string type, string identifier)
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
