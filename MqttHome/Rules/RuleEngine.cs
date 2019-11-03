using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;

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
            var switches = _controller.MqttDevices.Where(d => d is IStatefulDevice);
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
                var content = File.ReadAllText("rules.json");
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

                _controller.RuleLog.Info($"LoadRules :: Loaded {Config.Rules.Count} rules from rules.json");

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
                var sensors = _controller.MqttDevices.Where(d => d is ISensorDevice<ISensorData>).Select(d => d as ISensorDevice<ISensorData>);

                foreach (var rule in Config.Rules) {
                    try
                    {
                        // get the device
                        var s = _controller.MqttDevices.Where(d => d.Id == rule.Switch && d is IStatefulDevice).First() as IStatefulDevice;

                        // check the condition
                        var on = sensors.Any(rule.Condition);

                        if (on) {
                            if (!s.PowerOn)
                                s.SwitchOn($"RULE: {rule.Name}", rule.FlipFlop);
                        }
                        else {
                            if (s.PowerOn)
                                s.SwitchOff($"RULE: {rule.Name}");
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
