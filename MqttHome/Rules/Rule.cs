using System.Collections.Generic;
using System.Linq;

namespace MqttHome
{
    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Switch { get; set; }
        
        /// <summary>
        /// Any of these conditions can be matched to return true
        /// </summary>
        public List<string> ConditionsOr { get; set; }

        /// <summary>
        /// All of these conditions must be matched to return true
        /// </summary>
        public List<string> ConditionsAnd { get; set; }

        public int? FlipFlop { get; set; }

        public bool DependsOnCondition(string condition) {
            return (ConditionsOr.Any(c => c == condition) || ConditionsAnd.Any(c => c == condition));
        }

        /// <summary>
        /// Will return true if conditions are matched. Errs on the side of caution, if a condition value is null its presumed to be FALSE
        /// </summary>
        public bool Test(List<Condition> c) {
            bool result;

            var conditions = c.ToDictionary(o => o.Id, o => o.ConditionValue);

            foreach (var conditionId in ConditionsAnd)
            {
                result = conditions[conditionId] ?? false;

                // exit early if false
                if (!result)
                    return false;
            }

            foreach (var conditionId in ConditionsOr) {
                result = conditions[conditionId] ?? false;

                // exit early if true
                if (result)
                    return true;
            }

            return false;
        }
    }
}
