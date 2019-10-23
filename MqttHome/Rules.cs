using InfluxDbLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MqttHome
{
    public class RuleEngine
    {

        private RuleConfig _config = new RuleConfig();

        public RuleEngine()
        {
            LoadRules();

            ValidateRules();
        }

        public void Start() {
            Task.Run(() => RuleRunner());
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

                // append defaults and and/or conditions
                foreach (var rule in _config.Rules)
                {
                    // and/or conditions
                    var where = rule.Condition;

                    if (!string.IsNullOrEmpty(_config.Defaults.ConditionAnd))
                        where = $"({where}) AND {_config.Defaults.ConditionAnd}";

                    if (!string.IsNullOrEmpty(_config.Defaults.ConditionOr))
                        where = $"({where}) OR {_config.Defaults.ConditionOr}";

                    rule.Condition = where;

                    // flip flop defaults
                    if (!rule.FlipFlop.HasValue && _config.Defaults.FlipFlop.HasValue)
                        rule.FlipFlop = _config.Defaults.FlipFlop.Value;
                }

                Program.RuleLog.Info($"LoadRules :: Loaded {_config.Rules.Count} rules from rules.config");

            }
            catch (Exception err)
            {
                Program.RuleLog.Error($"LoadRules :: Failed to load rules. {err.Message}", err);
            }
        }

        private void RuleRunner()
        {
            while (true)
            {
                foreach (var rule in _config.Rules) {
                    try
                    {
                        var switches = Program.MqttDevices.Where(d => d.DeviceClass == InfluxDbLoader.Mqtt.MqttDeviceClass.Switch);
                        var ons = switches.Where(rule.Condition);
                        var offs = Program.MqttDevices.Except(ons);

                        // only switch of switches that are currently on
                        foreach (var off in offs.Where(d => d.PowerOn))
                        {
                            off.SwitchOff($"RULE: {rule.Name}");
                        }

                        // only switch on switches that are currently off
                        foreach (var on in ons.Where(d => !d.PowerOn))
                        {
                            on.SwitchOn($"RULE: {rule.Name}", rule.FlipFlop);
                        }
                    }
                    catch (Exception err) {
                        Program.RuleLog.Error($"RuleRunner :: Error in rule {rule.Name} - {err.Message}", err);
                    }
                }

                // wait 5 seconds before next loop - this will give the switches time to change state and report back -- needs to be monitored and refined
                Thread.Sleep(5000);
            }
        }
    }

    public class RuleConfig
    {
        public RuleDefaults Defaults { get; set; } = new RuleDefaults();
        public List<Rule> Rules { get; set; } = new List<Rule>();
    }

    public class RuleDefaults
    {
        public string ConditionOr { get; set; }
        public string ConditionAnd { get; set; }
        public int? FlipFlop { get; set; }
    }

    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Switch { get; set; }
        public string Condition { get; set; }
        public int? FlipFlop { get; set; }
    }
}
