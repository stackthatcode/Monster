using System;
using System.Collections.Generic;

namespace Push.Foundation.Utilities.Validation
{
    public class Validation<T>
    {
        public List<Rule<T>> Rules { get; set; }

        public Validation()
        {
            Rules = new List<Rule<T>>();
        }

        public Validation<T> Add(Rule<T> rule)
        {
            Rules.Add(rule);
            return this;
        }

        public Validation<T> Add(
                Func<T, bool> test, string validationMessage, bool instantFailure = false)
        {
            Rules.Add(new Rule<T>(test, validationMessage, instantFailure));
            return this;
        }

        public ValidationResult Run(T input)
        {
            var output = new ValidationResult();

            foreach (var rule in Rules)
            {
                if (rule.Test(input))
                {
                    continue;
                }
                output.FailureMessages.Add(rule.ValidationMessage);

                if (rule.InstantFailure)
                {
                    return output;
                }
            }
            return output;
        }
    }
}

