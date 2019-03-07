﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Models;
using Monster.Web.Models.RealTime;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class AnalysisController : Controller
    {
        private readonly SystemStateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly JobStatusService _jobStatusService;
        private readonly ConfigStatusService _statusService;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public AnalysisController(
                SystemStateRepository stateRepository,
                OneTimeJobService oneTimeJobService,
                RecurringJobService recurringJobService,
                JobStatusService jobStatusService,
                ConfigStatusService statusService,
                ExecutionLogService logRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                UrlService urlService,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _jobStatusService = jobStatusService;
            _statusService = statusService;
            _logRepository = logRepository;
            _urlService = urlService;
            _logger = logger;
        }

        
        [HttpGet]
        public ActionResult ExecutionLogs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExecutionLogData()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            return new JsonNetResult(logs);
        }        
    }
}
