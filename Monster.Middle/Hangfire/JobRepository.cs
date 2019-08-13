using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Hangfire
{
    public class JobRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public JobRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }

        
        public bool Exists(int backgroundJobType)
        {
            return RetrieveBackgroundJob(backgroundJobType) != null;
        }

        public BackgroundJob RetrieveBackgroundJob(int backgroundJobType)
        {
            return Entities
                .BackgroundJobs
                .FirstOrDefault(x => x.BackgroundJobType == backgroundJobType);
        }

        public List<BackgroundJob> RetrieveBackgroundJobs()
        {
            return Entities.BackgroundJobs.ToList();
        }

        public BackgroundJob
                    InsertBackgroundJob(int backgroundJobType, string hangFireJobId)
        {
            if (Exists(backgroundJobType))
            {
                RemoveBackgroundJobs(backgroundJobType);
            }
            
            var newJob = new BackgroundJob()
            {
                BackgroundJobType = backgroundJobType,
                HangFireJobId = hangFireJobId,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            Entities.BackgroundJobs.Add(newJob);
            Entities.SaveChanges();
            return newJob;
        }

        public void RemoveBackgroundJobs(int backgroundJobType)
        {
            var jobs = Entities
                .BackgroundJobs
                .Where(x => x.BackgroundJobType == backgroundJobType)
                .ToList();

            foreach (var job in jobs)
            {
                Entities.BackgroundJobs.Remove(job);
                Entities.SaveChanges();
            }
        }
    }
}
