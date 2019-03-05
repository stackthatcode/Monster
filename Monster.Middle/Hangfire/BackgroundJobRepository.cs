using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Tenant;

namespace Monster.Middle.Hangfire
{
    public class BackgroundJobRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public BackgroundJobRepository(PersistContext dataContext)
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

        public UsrBackgroundJob RetrieveBackgroundJob(int backgroundJobType)
        {
            return Entities
                .UsrBackgroundJobs
                .FirstOrDefault(x => x.BackgroundJobType == backgroundJobType);
        }

        public List<UsrBackgroundJob> RetrieveBackgroundJobs()
        {
            return Entities.UsrBackgroundJobs.ToList();
        }

        public UsrBackgroundJob
                    InsertBackgroundJob(int backgroundJobType, string hangFireJobId)
        {
            if (Exists(backgroundJobType))
            {
                RemoveBackgroundJobs(backgroundJobType);
            }
            
            var newJob = new UsrBackgroundJob()
            {
                BackgroundJobType = backgroundJobType,
                HangFireJobId = hangFireJobId,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            Entities.UsrBackgroundJobs.Add(newJob);
            Entities.SaveChanges();
            return newJob;
        }

        public void RemoveBackgroundJobs(int backgroundJobType)
        {
            var jobs = Entities
                .UsrBackgroundJobs
                .Where(x => x.BackgroundJobType == backgroundJobType)
                .ToList();

            foreach (var job in jobs)
            {
                Entities.UsrBackgroundJobs.Remove(job);
                Entities.SaveChanges();
            }
        }
    }
}
