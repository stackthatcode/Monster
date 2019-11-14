using System;

namespace Push.Foundation.Utilities.Validation
{
    public class Rule<T>
    {
        public Func<T, bool> Test { get; set; }
        public bool InstantFailure { get; set; }

        public Func<T, string> MessageBuilder { get; set; }
        public string ValidationMessageText { get; set; }

        public string ValidationMessage(T context)
        {
            return MessageBuilder != null  ? MessageBuilder(context) : ValidationMessageText;
        }


        public Rule(Func<T, bool> test, string validationMessageText, bool instantFailure = false)
        {
            Test = test;
            ValidationMessageText = validationMessageText;
            InstantFailure = instantFailure;
        }

        public Rule(Func<T, bool> test,  Func<T, string> messageBuilder, bool instantFailure = false)
        {
            Test = test;
            MessageBuilder = messageBuilder;
            InstantFailure = instantFailure;
        }
    }
}
