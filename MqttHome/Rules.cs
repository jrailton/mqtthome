using InfluxDbLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MqttHome
{
    public class RuleEngine
    {

        private RuleConfig _config;

        public RuleEngine()
        {
            LoadRules();

            ValidateRules();
        }

        private void ValidateRules()
        {
            // remove rules that dont apply to any switches
            var switches = Program.MqttDevices.Where(d => d.DeviceClass == InfluxDbLoader.Mqtt.MqttDeviceClass.Switch);
            var remove = _config.Rules.Where(r => !switches.Any(s => s.Id == r.Switch)).ToList();

            if (remove.Any())
            {
                // log it
                Program.RuleLog.Warn($"Removing {remove.Count} rules that refer to non-existent SWITCH devices: {string.Join(", ", remove.Select(r => r.Name))}");
                _config.Rules.RemoveAll(r => !switches.Any(s => s.Id == r.Switch));
            }
        }

        private void LoadRules()
        {
            try
            {
                var content = File.ReadAllText("rules.config");
                _config = JsonConvert.DeserializeObject<RuleConfig>(content);

                Program.RuleLog.Info($"Loaded {_config.Rules.Count} rules from rules.config");

            }
            catch (Exception err)
            {
                Program.RuleLog.Error($"Failed to load rules. {err.Message}", err);
            }
        }

        private void RuleRunner()
        {
            while (true)
            {
                foreach (var rule in _config.Rules) { 
                    if (Program.MqttDevices.Any(d => d.DeviceClass == InfluxDbLoader.Mqtt.MqttDeviceClass.Switch && ))
                }
                Thread.Sleep(1000);
            }
        }
    }

    public class RuleConfig
    {
        public RuleDefaults Defaults { get; set; }
        public List<Rule> Rules { get; set; }
    }

    public class RuleDefaults
    {
        public string ConditionOr { get; set; }
        public string ConditionAnd { get; set; }
        public string FlipFlop { get; set; }
    }

    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Switch { get; set; }
        public string Condition { get; set; }
        public string ConditionOr { get; set; }
        public string ConditionAnd { get; set; }
        public string FlipFlop { get; set; }
    }
}
