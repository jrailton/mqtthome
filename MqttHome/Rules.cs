using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MqttHome
{
    public class RuleEngine {

        private List<Rule> _rules;

        public RuleEngine() {
            LoadRules();
        }

        private void LoadRules() {
            var content = File.ReadAllLines("rules.config");

            foreach (var line in content) { 
                // omit comment lines and content with comments

            }
        }
    }
    public class Rule
    {
    }
}
