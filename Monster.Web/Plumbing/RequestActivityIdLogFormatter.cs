using System.Web;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace Monster.Web.Plumbing
{
    public class RequestActivityIdLogFormatter : ILogFormatter
    {
        // Important Note: Batch and Web follow different logging schemas
        // Web =>  Date|Level|ActivityId|Message
        // Batch => Date|Level|Message (message includes Trace Id)
        public string Do(string message)
        {
            var activityId = RequestActivityId.Current;
            var emailAddress = HttpContext.Current.ExtractUserEmail().IsNullOrEmptyAlt("(No User Email)");
            return $"{activityId} - {emailAddress}|{message ?? ""}";
        }
    }
}

