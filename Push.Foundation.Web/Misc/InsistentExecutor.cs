using System;
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

            while (true)
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    counter++;

                    if (counter > MaxNumberOfAttempts)
                    {
                        _logger.Fatal(
                            $"Attempts has exceeded limit {MaxNumberOfAttempts}... throwing exception");
                        throw;
                    }
                    else
                    {
                        _logger.Info($"Encountered an exception on attempt {counter - 1}. Retrying invocation...");
                    }
                }
            }
        }
    }
}
