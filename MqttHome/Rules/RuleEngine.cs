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
using System.Collections.Specialized;
using MqttHome.Presence;

namespace MqttHome
{
    public class RuleEngine
    {
        public RuleConfig RuleConfig { get; private set; } = new RuleConfig();
        public ConditionConfig ConditionConfig { get; private set; } = new ConditionConfig();
        private MqttHomeController _controller;

        public List<Notification> RuleValidationNotifications = new List<Notification>();
        public List<Notification> ConditionValidationNotifications = new List<Notification>();

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
            var switches = _controller.MqttDevices.Where(d => d is ISwitchDevice);
            var remove = RuleConfig.Rules.Where(r => !switches.Any(s => s.Id == r.Switch)).ToList();

            if (remove.Any())
            {
                // log it
                remove.ForEach(r => RuleValidationNotifications.Add(new Notification("warning", $"{r.Name} refers to a switch ({r.Switch}) that does not exist")));

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
                remove.ForEach(r => RuleValidationNotifications.Add(new Notification("warning", $"{r.Name} either does not have any valid conditions or refers to a condition that does not exist")));
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
                    if (RuleConfig.Defaults.ConditionsOr?.Any() ?? false)
                        rule.ConditionsOr.AddRange(RuleConfig.Defaults.ConditionsOr);

                    // flip flop defaults
                    if (!rule.FlipFlop.HasValue && RuleConfig.Defaults.FlipFlop.HasValue)
                        rule.FlipFlop = RuleConfig.Defaults.FlipFlop.Value;
                }

                _controller.RuleLog.Info($"LoadRules :: Loaded {RuleConfig.Rules.Count} rules from rules.json");

            }
            catch (Exception err)
            {
                ConditionValidationNotifications.Add(new Notification("danger", $"Failed to load rules - {err.Message}"));
                _controller.RuleLog.Error($"LoadRules :: Failed to load rules. {err.Message}", err);
            }
        }

        private void LoadConditions()
        {
            var logIdentity = "Parsing conditions JSON";
            try
            {
                var content = File.ReadAllText("conditions.json");
                ConditionConfig = JsonConvert.DeserializeObject<ConditionConfig>(content);

                // validate each rule and attach event to each
                foreach (var condition in ConditionConfig.Conditions)
                {
                    logIdentity = $"Setting up condition {condition.Id}";

                    // validate condition
                    var problems = condition.CheckProblems();

                    if (problems.Any())
                    {
                        // log it
                        problems.ForEach(p => ConditionValidationNotifications.Add(new Notification("warning", $"{condition.Id} has a problem: {p}")));
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
                ConditionValidationNotifications.Add(new Notification("danger", $"Failed to load conditions - {err.Message}"));
                _controller.RuleLog.Error($"LoadConditions :: {logIdentity} :: Failed to load rules. {err.Message}", err);
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

                    var switchDevice = _controller.MqttDevices.OfType<ISwitchDevice>().FirstOrDefault(d => d.Id == rule.Switch);

                    logIdentity += $" :: Test Conditions";

                    var ruleState = rule.Test(ConditionConfig.Conditions, _controller.RuleLog);

                    if (rule.State != ruleState)
                    {
                        logIdentity += $" :: Rule state changed to {ruleState}";
                        rule.State = ruleState;
                        rule.StateChanged = DateTime.Now;
                    }

                    // dont attach the event if rule engine disabled
                    if (_controller.RuleEngineEnabled)
                    {
                        if (ruleState)
                        {
                            // only switch device ON if its currently OFF (presume its OFF if no power state set yet)
                            if (!(switchDevice.PowerOn ?? false))
                            {
                                logIdentity += $" :: Device ID {switchDevice.Id} is OFF. Turning it ON";

                                switchDevice.SwitchOn($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})", rule.FlipFlop);
                            }
                        }
                        else
                        {
                            // only switch device OFF if its currently ON (presume its ON if no power state set yet)
                            if (switchDevice.PowerOn ?? true)
                            {
                                logIdentity += $" :: Device ID {switchDevice.Id} is ON. Turning it OFF";

                                switchDevice.SwitchOff($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})");
                            }
                        }
                    }
                    else {
                        _controller.RuleLog.Warn($"{logIdentity} - Cancelling, rule engine disabled by appsettings.json");
                    }
                }
                catch (Exception err)
                {
                    _controller.RuleLog.Error($"{logIdentity} :: Failed - {err.Message}", err);
                }
            }
        }

        public void OnDeviceStateChanged(MqttDevice device, StateChangedEventArgs e)
        {
        }

        public void OnDeviceSensorDataChanged(IMqttSensorDevice<ISensorData> device, Dictionary<string, object> allSensorValues)
        {
            foreach (var condition in ConditionConfig.Conditions.Where(c => c.Device == device.Id))
            {
                try
                {
                    condition.CheckDeviceCondition(device, allSensorValues);
                }
                catch (Exception err)
                {
                    _controller.RuleLog.Error($"OnDeviceSensorDataChanged :: CheckDeviceCondition :: Condition: {condition.Id}, Device: {device.Id} - Failed. {err.Message}", err);
                }
            }
        }

        public void OnPresenceChanged(Person person) {
            foreach (var condition in ConditionConfig.Conditions.Where(c => c.People?.Contains(person.Id) ?? false))
            {
                try
                {
                    condition.CheckPeopleCondition(_controller.People);
                }
                catch (Exception err)
                {
                    _controller.RuleLog.Error($"OnPresenceChanged :: CheckPeopleCondition :: Condition: {condition.Id}, Person: {person.Id} - Failed. {err.Message}", err);
                }
            }

        }
    }
}
