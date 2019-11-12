using System.Collections.Generic;

namespace MqttHome
{
    public class RuleConfig
    {
        public RuleDefaults Defaults { get; set; } = new RuleDefaults();
        public List<Rule> Rules { get; set; } = new List<Rule>();
    }
}
