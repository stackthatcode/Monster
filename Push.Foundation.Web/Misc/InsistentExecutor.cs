using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class InsistentExecutor
    {
        private readonly IPushLogger _logger;
        public int MaxNumberOfAttempts { get; set; }

        public InsistentExecutor(IPushLogger logger)
        {
            MaxNumberOfAttempts = 1;
            _logger = logger;
        }

        public InsistentExecutor()
        {
            MaxNumberOfAttempts = 1;
            _logger = new ConsoleAndDebugLogger();
        }
        
        public virtual T Execute<T>(Func<T> function)
        {
            var counter = 1;
            var exceptions = new List<Exception>();

            while (true)
            {
                try
                {
                    if (exceptions.Count > 0)
                        _logger.Warn(exceptions, "Invocation succeeded, but encountered errors...");

                    return function();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    counter++;
                    
                    if (counter > MaxNumberOfAttempts)
                    {
                        _logger.Warn(exceptions, "Retry Limit has been exceeded... throwing exception");
                        throw;
                    }
                    else
                    {
                        _logger.Debug("Encountered an exception. Retrying invocation...");
                    }
                }
            }
        }
    }
}
