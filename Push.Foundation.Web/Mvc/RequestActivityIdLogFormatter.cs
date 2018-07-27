using System.Web;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Helpers
{
    public class RequestActivityIdLogFormatter : ILogFormatter
    {
        // Important Note: Batch and Web follow different logging schemas
        // Web =>  Date|Level|ActivityId|Message
        // Batch => Date|Level|Message (message includes Trace Id)
        public string Do(string message)
        {
            var activityId = RequestActivityId.Current;
            var userId = HttpContext.Current.ExtractUserId().IsNullOrEmptyAlt("(No User Id)");
            return $"{activityId} - {userId}|{message ?? ""}";
        }
    }
}

