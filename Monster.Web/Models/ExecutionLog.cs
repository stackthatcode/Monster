using Monster.Middle.Persist.Multitenant;

namespace Monster.Web.Models
{
    public class ExecutionLog
    {
        public string LogTime { get; set; }
        public string Content { get; set; }

        public ExecutionLog(UsrExecutionLog entity)
        {
            LogTime = entity.DateCreated.ToString();
            Content = entity.LogContent;
        }
    }
}
