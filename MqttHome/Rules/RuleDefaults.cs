using System.Collections.Generic;

namespace MqttHome
{
    public class RuleDefaults
    {
        /// <summary>
        /// Any of these conditions can be matched to return true
        /// </summary>
        public List<string> ConditionsOr { get; set; }

        /// <summary>
        /// All of these conditions must be matched to return true
        /// </summary>
        public List<string> ConditionsAnd { get; set; }

        public int? FlipFlop { get; set; }
    }
}
