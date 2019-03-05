using System;

namespace Push.Foundation.Utilities.Validation
{
    public class Rule<T>
    {
        public Func<T, bool> Test { get; set; }
        public bool InstantFailure { get; set; }
        public string ValidationMessage { get; set; }

        public Rule(Func<T, bool> test, string validationMessage, bool instantFailure = false)
        {
            Test = test;
            ValidationMessage = validationMessage;
            InstantFailure = instantFailure;
        }
    }
}
