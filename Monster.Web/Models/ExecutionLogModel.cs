using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Web.Models
{
    public class ExecutionLogModel
    {
        public string LogTime { get; set; }
        public string Content { get; set; }

        public ExecutionLogModel(UsrExecutionLog entity)
        {
            LogTime = entity.DateCreated.ToString();
            Content = entity.LogContent;
        }
    }

    public static class ExecutionLogModelExtensions
    {
        public static List<ExecutionLogModel> 
                ToModel(this IEnumerable<UsrExecutionLog> input)
        {
            return input.Select(x => new ExecutionLogModel(x)).ToList();
        }
    }
}
