using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class ExceptionAbsorber
    {        
        public static T Do<T>(
                Func<T> function, int maxNumberOfAttempts = 1, 
                IPushLogger logger = null)
        {
            var counter = 1;
            var exceptions = new List<Exception>();
            if (logger == null)
            {
                logger = new ConsoleAndDebugLogger();
            }

            while (true)
            {
                try
                {
                    if (exceptions.Count > 0)
                    {
                        logger.Warn(exceptions, "Invocation succeeded, but encountered errors...");
                    }

                    return function();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    counter++;
                    
                    if (counter > maxNumberOfAttempts)
                    {
                        logger.Warn(exceptions, "Retry Limit has been exceeded... throwing exception");
                        throw;
                    }
                    else
                    {
                        logger.Debug("Encountered an exception. Retrying invocation...");
                    }
                }
            }
        }
    }
}
