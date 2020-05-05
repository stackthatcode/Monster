using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Push.Foundation.Utilities.Helpers;
using Sentry;

namespace Push.Foundation.Utilities.Logging
{
    public class NLogger : IPushLogger
    {
        private readonly Logger _nLoggerReference;
        private readonly ILogFormatter _formatter;
        private readonly bool _sentryEnabled;
        
        public NLogger(string loggerName, bool sentryEnabled = false, ILogFormatter formatter = null)
        {
            _formatter = formatter ?? new DefaultFormatter();
            _nLoggerReference = LogManager.GetLogger(loggerName);
            _sentryEnabled = sentryEnabled;
        }
        
        public bool IsTraceEnabled => _nLoggerReference.IsTraceEnabled;
        public bool IsDebugEnabled => _nLoggerReference.IsDebugEnabled;
        public bool IsInfoEnabled => _nLoggerReference.IsInfoEnabled;
        public bool IsWarnEnabled => _nLoggerReference.IsWarnEnabled;
        public bool IsErrorEnabled => _nLoggerReference.IsErrorEnabled;
        public bool IsFatalEnabled => _nLoggerReference.IsFatalEnabled;


        public void Trace(string message)
        {
            _nLoggerReference.Trace(_formatter.Do(message));
        }

        public void Debug(string message)
        {
            _nLoggerReference.Debug(_formatter.Do(message));
        }

        public void Info(string message)
        {
            _nLoggerReference.Info(_formatter.Do(message));
        }

        public void Warn(string message)
        {
            _nLoggerReference.Warn(_formatter.Do(message));
        }

        public void Warn(List<Exception> exceptions, string message = null)
        {
            if (exceptions == null)
            {
                return;
            }

            var header =
                message.IsNullOrEmpty() ? "" : message + Environment.NewLine;

            var body =
                exceptions
                    .Select(x => x.FullStackTraceDump())
                    .StringJoin(Environment.NewLine);

            var entry = header + body;

            _nLoggerReference.Warn(_formatter.Do(entry));
        }

        public void Error(string message)
        {
            _nLoggerReference.Error(_formatter.Do(message));
        }

        public void Error(Exception exception, string message)
        {
            _nLoggerReference.Error(
                _formatter.Do(exception.FullStackTraceDump()) + Environment.NewLine + message);

            if (_sentryEnabled)
            {
                SentrySdk.CaptureException(exception);
            }
        }

        public void Error(Exception exception)
        {
            _nLoggerReference.Error(_formatter.Do(exception.FullStackTraceDump()));

            if (_sentryEnabled)
            {
                SentrySdk.CaptureException(exception);
            }
        }

        public void Fatal(string message)
        {
            _nLoggerReference.Fatal(_formatter.Do(message));
        }

        public void Fatal(Exception exception)
        {
            _nLoggerReference.Fatal(_formatter.Do(exception.FullStackTraceDump()));
        }
    }
}
 