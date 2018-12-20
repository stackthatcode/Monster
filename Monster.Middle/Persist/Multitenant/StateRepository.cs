﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Monster.Middle.Persist.Multitenant.Model;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Persist.Multitenant
{
    public class StateRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public StateRepository(PersistContext dataContext)
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

        public void RemoveBackgroundJob(int backgroundJobType)
        {
            var job = RetrieveBackgroundJob(backgroundJobType);
            Entities.UsrBackgroundJobs.Remove(job);
            Entities.SaveChanges();
        }



        static readonly object SystemStateLock = new object();

        public void EnsureSystemState()
        {
            lock (SystemStateLock)
            {
                if (!Entities.UsrSystemStates.Any())
                {
                    var newRecord = new UsrSystemState();
                    newRecord.ShopifyConnection = SystemState.None;
                    newRecord.AcumaticaConnection = SystemState.None;
                    newRecord.AcumaticaConfig = SystemState.None;
                    newRecord.WarehouseSync = SystemState.None;
                    newRecord.AcumaticaInventoryPush = SystemState.None;
                    newRecord.ShopifyInventoryPush = SystemState.None;
                    newRecord.IsShopifyUrlFinalized = false;
                    newRecord.IsAcumaticaUrlFinalized = false;
                    newRecord.RealTimeHangFireJobId = null;

                    Entities.UsrSystemStates.Add(newRecord);
                    Entities.SaveChanges();
                }
            }
        }
        
        public UsrSystemState RetrieveSystemState()
        {
            EnsureSystemState();
            return Entities.UsrSystemStates.First();
        }

        public void UpdateSystemState(
                Expression<Func<UsrSystemState, int>> memberExpression,
                int newValue)
        {
            var state = RetrieveSystemState();
            state.SetPropertyValue(memberExpression, newValue);
            Entities.SaveChanges();
        }


        public void InsertExecutionLog(string content)
        {
            var logEntry = new UsrExecutionLog();
            logEntry.LogContent = content;
            logEntry.DateCreated = DateTime.UtcNow;
            Entities.UsrExecutionLogs.Add(logEntry);
            Entities.SaveChanges();
        }

        public List<UsrExecutionLog> RetrieveExecutionLogs()
        {
            return Entities
                .UsrExecutionLogs
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }


    }
}