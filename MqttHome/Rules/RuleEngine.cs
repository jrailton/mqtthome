using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;
using System.Collections.Generic;

namespace MqttHome
{
    public class RuleEngine
    {
        public RuleConfig RuleConfig { get; private set; } = new RuleConfig();
        public ConditionConfig ConditionConfig { get; private set; } = new ConditionConfig();
        private MqttHomeController _controller;

        public RuleEngine(MqttHomeController controller)
        {
            _controller = controller;

            LoadConditions();

            LoadRules();

            ValidateRules();
        }

        private void ValidateRules()
        {
            // remove rules that dont apply to any switches
            var switches = _controller.MqttDevices.Where(d => d is IStatefulDevice);
            var remove = RuleConfig.Rules.Where(r => !switches.Any(s => s.Id == r.Switch)).ToList();

            if (remove.Any())
            {
                // log it
                _controller.RuleLog.Warn($"ValidateRules :: Removing {remove.Count} rules that refer to non-existent SWITCH devices: {string.Join(", ", remove.Select(r => r.Name))}");
                RuleConfig.Rules.RemoveAll(r => !switches.Any(s => s.Id == r.Switch));
            }

            // remove rules that refer to non-existent conditions or have no conditions
            remove = RuleConfig.Rules.Where(r =>
                (!r.ConditionsAnd.Any() && !r.ConditionsOr.Any()) ||
                r.ConditionsAnd.Any(ca => !ConditionConfig.Conditions.Any(c => c.Id == ca)) ||
                r.ConditionsOr.Any(co => !ConditionConfig.Conditions.Any(c => c.Id == co))
            ).ToList();

            if (remove.Any())
            {
                // log it
                _controller.RuleLog.Warn($"ValidateRules :: Removing {remove.Count} rules that refer to non-existent conditions OR have no conditions specified: {string.Join(", ", remove.Select(r => r.Name))}");
                RuleConfig.Rules.RemoveAll(r =>
                    (!r.ConditionsAnd.Any() && !r.ConditionsOr.Any()) ||
                    r.ConditionsAnd.Any(ca => !ConditionConfig.Conditions.Any(c => c.Id == ca)) ||
                    r.ConditionsOr.Any(co => !ConditionConfig.Conditions.Any(c => c.Id == co))
                );
            }

        }

        private void LoadRules()
        {
            try
            {
                var content = File.ReadAllText("rules.json");
                RuleConfig = JsonConvert.DeserializeObject<RuleConfig>(content);

                // append defaults to rules
                foreach (var rule in RuleConfig.Rules)
                {
                    // AND conditions
                    if (RuleConfig.Defaults.ConditionsAnd?.Any() ?? false)
                        rule.ConditionsAnd.AddRange(RuleConfig.Defaults.ConditionsAnd);

                    // OR conditions
                    if (RuleConfig.Defaults.ConditionsAnd?.Any() ?? false)
                        rule.ConditionsAnd.AddRange(RuleConfig.Defaults.ConditionsAnd);

                    // flip flop defaults
                    if (!rule.FlipFlop.HasValue && RuleConfig.Defaults.FlipFlop.HasValue)
                        rule.FlipFlop = RuleConfig.Defaults.FlipFlop.Value;
                }

                _controller.RuleLog.Info($"LoadRules :: Loaded {RuleConfig.Rules.Count} rules from rules.json");

            }
            catch (Exception err)
            {
                _controller.RuleLog.Error($"LoadRules :: Failed to load rules. {err.Message}", err);
            }
        }

        private void LoadConditions()
        {
            try
            {
                var content = File.ReadAllText("conditions.json");
                ConditionConfig = JsonConvert.DeserializeObject<ConditionConfig>(content);

                // validate each rule and attach event to each
                foreach (var condition in ConditionConfig.Conditions) {

                    var problems = condition.CheckProblems();
                    if (problems.Any())
                    {
                        _controller.RuleLog.Error($"LoadConditions :: Condition ID {condition.Id} has problems: {string.Join(Environment.NewLine, problems)}");
                    }
                    else
                    {
                        condition.ConditionValueChanged += OnConditionValueChanged;
                    }
                }

                // remove dodgy conditions
                ConditionConfig.Conditions.RemoveAll(c => c.CheckProblems().Any());

                _controller.RuleLog.Info($"LoadConditions :: Loaded {ConditionConfig.Conditions.Count} conditions from conditions.json");

            }
            catch (Exception err)
            {
                _controller.RuleLog.Error($"LoadConditions :: Failed to load rules. {err.Message}", err);
            }
        }

        private void OnConditionValueChanged(object sender, EventArgs e)
        {
            var condition = (Condition)sender;

            string logIdentity = $"OnConditionValueChanged :: Condition ID {condition.Id}";

            foreach (var rule in RuleConfig.Rules.Where(r => r.DependsOnCondition(condition.Id)))
            {
                logIdentity += $" :: Rule {rule.Name}";

                try
                {
                    logIdentity += $" :: Find SWITCH device {rule.Switch}";

                    var statefulDevice = _controller.MqttDevices.OfType<IStatefulDevice>().FirstOrDefault(d => d.Id == rule.Switch);

                    logIdentity += $" :: Test Conditions";

                    var ruleState = rule.Test(ConditionConfig.Conditions);

                    if (rule.State != ruleState)
                    {
                        logIdentity += $" :: Rule state changed to {ruleState}";
                        rule.State = ruleState;
                        rule.StateChanged = DateTime.Now;
                    }

                    if (ruleState)
                    {
                        // only switch device ON if its currently OFF (presume its OFF if no power state set yet)
                        if (!(statefulDevice.PowerOn ?? false))
                        {
                            logIdentity += $" :: Device ID {statefulDevice.Id} is OFF. Turning it ON";

                            statefulDevice.SwitchOn($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})", rule.FlipFlop);
                        }
                    }
                    else
                    {
                        // only switch device OFF if its currently ON (presume its ON if no power state set yet)
                        if (statefulDevice.PowerOn ?? true)
                        {
                            logIdentity += $" :: Device ID {statefulDevice.Id} is ON. Turning it OFF";

                            statefulDevice.SwitchOff($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})");
                        }
                    }
                }
                catch (Exception err) {
                    _controller.RuleLog.Error($"{logIdentity} :: Failed - {err.Message}", err);
                }
            }
        }

        public void OnDeviceStateChanged(MqttDevice device, StateChangedEventArgs e)
        {
        }

        public void OnDeviceSensorDataChanged(ISensorDevice<ISensorData> device, Dictionary<string, object> allSensorValues)
        {
            foreach (var condition in ConditionConfig.Conditions.Where(c => c.Device == device.Id))
            {
                try
                {
                    condition.CheckCondition(device, allSensorValues);
                }
                catch (Exception err) {
                    _controller.RuleLog.Error($"OnDeviceSensorDataChanged :: CheckCondition :: Condition: {condition.Id}, Device: {device.Id} - Failed. {err.Message}", err);
                }
            }
        }
    }
}
