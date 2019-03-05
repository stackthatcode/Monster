using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Hangfire
{
    public class JobStatusService
    {
        private readonly SystemStateRepository _stateRepository;

        public JobStatusService(SystemStateRepository stateRepository)
        {
            _stateRepository = stateRepository;
        }


        // Summary methods
        //
        public bool AreAnyBackgroundJobsRunning()
        {
            var jobs = _stateRepository.RetrieveBackgroundJobs();
            var anyOneTimeJobs = jobs.Any(x => IsHangFireFafJobRunning(x.HangFireJobId));
            var anyRecurringJobs = IsRealTimeSyncRunning();

            return anyOneTimeJobs || anyRecurringJobs;
        }
        


        // Job Type-specific methods
        //
        public bool IsOneTimeJobRunning(int jobType)
        {
            // If Background Job Record is missing, return false
            var jobRecord = _stateRepository.Entities.RetrieveBackgroundJob(jobType);
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
            var state = _stateRepository.RetrieveSystemState();
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
        }
    }
}
