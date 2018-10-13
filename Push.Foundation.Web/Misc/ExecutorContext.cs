﻿using System;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class ExecutorContext
    {
        public IPushLogger Logger { get; set; }
        public int NumberOfAttempts { get; set; }
        public string ThrottlingKey { get; set; }

        public ExecutorContext()
        {
            Logger = new ConsoleAndDebugLogger();
            NumberOfAttempts = 1;
            ThrottlingKey = Guid.NewGuid().ToString();
        }
    }
}