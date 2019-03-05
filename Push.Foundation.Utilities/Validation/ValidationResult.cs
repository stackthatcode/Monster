using System.Collections.Generic;
using System.Linq;

namespace Push.Foundation.Utilities.Validation
{

    public class ValidationResult
    {
        public bool Success => !FailureMessages.Any();
        public List<string> FailureMessages { get; set; }

        public ValidationResult()
        {
            FailureMessages = new List<string>();
        }

        public static ValidationResult InstantFailure(string reason)
        {
            return new ValidationResult()
            {
                FailureMessages = new List<string> { reason },
            };
        }
    }
}
