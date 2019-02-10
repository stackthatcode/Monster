using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class ExecutionLogRepository
    {
        private readonly PersistContext _dataContext;

        public ExecutionLogRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public MonsterDataContext Entities => _dataContext.Entities;

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

    }
}
