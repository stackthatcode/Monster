using System;

namespace Push.Foundation.Utilities.Logging
{
    // Important in Autofac, the registration needs to scope this specifically to the 
    // ... lifetime e.g. HTTP hit, or Hangfire process
    public class ScopedTraceFormatter : ILogFormatter
    {        
        public string ScopedPrefix { get; private set; }

        public ScopedTraceFormatter()
        {
            this.ScopedPrefix = $"TraceId:{UtilityExtensions.RandomHexadecimal(6).ToLower()}";
        }

        public string Do(string message)
        {
            return $"{ScopedPrefix} - {UtilityExtensions.TypeAndMethodName()}|{message}";
        }
    }
}
