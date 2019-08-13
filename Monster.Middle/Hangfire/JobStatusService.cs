using System.Linq;
using Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Hangfire
{
    public class JobStatusService
    {
        private readonly StateRepository _stateRepository;
        private readonly JobRepository _jobRepository;

        public JobStatusService(
                StateRepository stateRepository, JobRepository jobRepository)
        {
            _stateRepository = stateRepository;
            _jobRepository = jobRepository;
        }


        // Summary methods
        //
        public bool AreAnyBackgroundJobsRunning()
        {
            var jobs = _jobRepository.RetrieveBackgroundJobs();
            var anyOneTimeJobs = jobs.Any(x => IsHangFireFafJobRunning(x.HangFireJobId));
            var anyRecurringJobs = IsRealTimeSyncRunning();

            return anyOneTimeJobs || anyRecurringJobs;
        }
        


        // Job Type-specific methods
        //
        public bool IsOneTimeJobRunning(int jobType)
        {
            // If Background Job Record is missing, return false
            var jobRecord = _jobRepository.RetrieveBackgroundJob(jobType);
            if (jobRecord == null)
            {
                return false;
            }
            else
            {
                return IsHangFireFafJobRunning(jobRecord.HangFireJobId);
            }
        }

        public bool IsHangFireFafJobRunning(string hangfireJobId)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobData = connection.GetJobData(hangfireJobId);
                return jobData.IsRunning();
            }
        }
        
        public bool IsRealTimeSyncRunning()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
        }
    }
}
