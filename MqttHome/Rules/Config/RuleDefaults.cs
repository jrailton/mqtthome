using System.Collections.Generic;
using System.Linq;

namespace MqttHome
{
    public class RuleDefaults
    {
        /// <summary>
        /// Any of these conditions can be matched to return true
        /// </summary>
        public List<string> ConditionsOr { get; set; } = new List<string>();

        /// <summary>
        /// All of these conditions must be matched to return true
        /// </summary>
        public List<string> ConditionsAnd { get; set; } = new List<string>();

        public int? FlipFlop { get; set; }

        public bool Any()
        {
            return FlipFlop.HasValue || ConditionsOr.Any() || ConditionsAnd.Any();
        }
    }
}
