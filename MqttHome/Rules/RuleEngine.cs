using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using MqttHome.Mqtt;

namespace MqttHome
{
    public class RuleEngine
    {
        public RuleConfig Config { get; private set; } = new RuleConfig();
        private MqttHomeController _controller;

        public RuleEngine(MqttHomeController controller)
        {
            _controller = controller;

            LoadRules();

            ValidateRules();
        }

        public void Start() {
            Task.Run(() => RuleRunner());
        }

        private void ValidateRules()
        {
            // remove rules that dont apply to any switches
            var switches = _controller.MqttDevices.Where(d => d.DeviceClass == MqttDeviceClass.Switch);
            var remove = Config.Rules.Where(r => !switches.Any(s => s.Id == r.Switch)).ToList();

            if (remove.Any())
            {
                // log it
                _controller.RuleLog.Warn($"Removing {remove.Count} rules that refer to non-existent SWITCH devices: {string.Join(", ", remove.Select(r => r.Name))}");
                Config.Rules.RemoveAll(r => !switches.Any(s => s.Id == r.Switch));
            }
        }

        private void LoadRules()
        {
            try
            {
                var content = File.ReadAllText("rules.config");
                Config = JsonConvert.DeserializeObject<RuleConfig>(content);

                // append defaults and and/or conditions
                foreach (var rule in Config.Rules)
                {
                    // and/or conditions
                    var where = rule.Condition;

                    if (!string.IsNullOrEmpty(Config.Defaults.ConditionAnd))
                        where = $"({where}) AND {Config.Defaults.ConditionAnd}";

                    if (!string.IsNullOrEmpty(Config.Defaults.ConditionOr))
                        where = $"({where}) OR {Config.Defaults.ConditionOr}";

                    rule.Condition = where;

                    // flip flop defaults
                    if (!rule.FlipFlop.HasValue && Config.Defaults.FlipFlop.HasValue)
                        rule.FlipFlop = Config.Defaults.FlipFlop.Value;
                }

                _controller.RuleLog.Info($"LoadRules :: Loaded {Config.Rules.Count} rules from rules.config");

            }
            catch (Exception err)
            {
                _controller.RuleLog.Error($"LoadRules :: Failed to load rules. {err.Message}", err);
            }
        }

        private void RuleRunner()
        {
            while (true)
            {
                foreach (var rule in Config.Rules) {
                    try
                    {
                        var switches = _controller.MqttDevices.Where(d => d.DeviceClass == MqttDeviceClass.Switch);
                        var ons = switches.Where(rule.Condition);
                        var offs = _controller.MqttDevices.Except(ons);

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
                        _controller.RuleLog.Error($"RuleRunner :: Error in rule {rule.Name} - {err.Message}", err);
                    }
                }

                // wait 5 seconds before next loop - this will give the switches time to change state and report back -- needs to be monitored and refined
                Thread.Sleep(5000);
            }
        }
    }
}
