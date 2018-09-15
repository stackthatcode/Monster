using System;

namespace Push.Foundation.Web.Misc
{
    public class DurableExecutor
    {
        
        public static T Do<T>(Func<T> task, ExecutionContext context)
        {
            if (context == null)
            {
                context = new ExecutionContext();
            }
            
            // Invoke the Throttler
            Throttler.Process(context.ThrottlingKey);
            
            // Do Request and process the HTTP Status Codes
            var startTime = DateTime.UtcNow;
            var result =
                ExceptionAbsorber.Do(
                    task, context.NumberOfAttempts, context.Logger);
            
            // Log execution time            
            var executionTime = DateTime.UtcNow - startTime;
            context.Logger.Debug($"Call performance - {executionTime} ms");
            
            return result;
        }        
    }
}


