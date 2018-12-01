using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant.Model;

namespace Monster.Middle.Persist.Multitenant
{
    public class JobRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public JobRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public void Clear()
        {
            Entities.UsrQueuedJobs.RemoveRange(Entities.UsrQueuedJobs.ToList());
        }

        public bool PendingExists(int queuedJobType)
        {
            var job = Retrieve(queuedJobType);
            return job.JobStatus == JobStatus.Pending;
        }

        public UsrQueuedJob Retrieve(int queuedJobType)
        {
            return Entities
                .UsrQueuedJobs
                .FirstOrDefault(x => x.QueuedJobType == queuedJobType);
        }

        public UsrQueuedJob SetPending(int queuedJobType, string hangFireJobId)
        {
            var job = Retrieve(queuedJobType);

            if (job != null)
            {
                Entities.UsrQueuedJobs.Remove(job);
            }

            var newJob = new UsrQueuedJob()
            {
                QueuedJobType = queuedJobType,
                JobStatus = JobStatus.Pending,
                HangFireJobId = hangFireJobId,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            Entities.UsrQueuedJobs.Add(newJob);
            Entities.SaveChanges();
            return newJob;
        }
        
        public void UpdateStatus(int queuedJobType, int jobStatus)
        {
            var job = Retrieve(queuedJobType);
            job.JobStatus = jobStatus;
            job.LastUpdated = DateTime.UtcNow;
            Entities.SaveChanges();
        }
        
    }
}
