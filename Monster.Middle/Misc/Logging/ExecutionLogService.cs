using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Misc.Logging
{
    public class ExecutionLogService
    {
        private readonly MiscPersistContext _dataContext;
        private readonly IPushLogger _logger;

        private MonsterDataContext Entities => _dataContext.Entities;

        public ExecutionLogService(MiscPersistContext dataContext, IPushLogger logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public void Log(string content, int level = LogLevel.Information)
        {
            try
            {
                if (level == LogLevel.Information)
                {
                    _logger.Info(content);
                }
                if (level == LogLevel.Debug)
                {
                    _logger.Debug(content);
                }
                if (level == LogLevel.Error)
                {
                    _logger.Error(content);
                }

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
    }
}
