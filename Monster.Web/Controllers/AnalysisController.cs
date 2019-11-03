﻿using System.Web.Mvc;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class AnalysisController : Controller
    {
        private readonly ExecutionLogService _logRepository;

        public AnalysisController(ExecutionLogService logRepository)
        {
            _logRepository = logRepository;
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

        [HttpGet]
        public ActionResult OrderSync()
        {
            return View();
        }
    }
}

