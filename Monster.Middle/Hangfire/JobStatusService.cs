using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Hangfire
{
    public class JobStatusService
    {
        private readonly StateRepository _stateRepository;

        public JobStatusService(StateRepository stateRepository)
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
        
        public bool AreAnyOneTimeJobsRunning(List<int> jobTypes)
        {
            return jobTypes.Any(x => IsOneTimeJobRunning(x));
        }
        


        // Job Type-specific methods
        //
        public bool IsOneTimeJobRunning(int jobType)
        {
            // If Background Job Record is missing, return false
            var jobRecord = _stateRepository.RetrieveBackgroundJob(jobType);
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
            
            if (state.RealTimeHangFireJobId.IsNullOrEmpty())
            {
                return false;
            }

            using (var connection = JobStorage.Current.GetConnection())
            {
                var recurringJobs = connection.GetRecurringJobs();
                var job = recurringJobs
                            .FirstOrDefault(p => p.Id == state.RealTimeHangFireJobId);

                if (job == null || job.Job == null)
                {
                    return false;
                }

                try
                {
                    var jobState = connection.GetStateData(job.Id);
                    return jobState.IsRunning();
                }
                catch (Exception ex)
                {
                    //job has not been run by the scheduler yet, swallow error
                    return false;
                }
            }.
        }
    }
}
