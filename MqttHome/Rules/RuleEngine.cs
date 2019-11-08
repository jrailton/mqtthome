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
                _controller.RuleLog.Warn($"Removing {remove.Count} rules that refer to non-existent SWITCH devices: {string.Join(", ", remove.Select(r => r.Name))}");
                RuleConfig.Rules.RemoveAll(r => !switches.Any(s => s.Id == r.Switch));
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

                foreach (var condition in ConditionConfig.Conditions) {
                    condition.ConditionValueChanged += OnConditionValueChanged;
                }

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

            foreach (var rule in RuleConfig.Rules.Where(r => r.DependsOnCondition(condition.Id)))
            {
                var statefulDevice = _controller.MqttDevices.OfType<IStatefulDevice>().FirstOrDefault(d => d.Id == rule.Switch);

                if (rule.Test(ConditionConfig.Conditions))
                {
                    // only switch device ON if its currently OFF (presume its OFF if no power state set yet)
                    if (!(statefulDevice.PowerOn ?? false))
                        statefulDevice.SwitchOn($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})", rule.FlipFlop);
                }
                else
                {
                    // only switch device OFF if its currently ON (presume its ON if no power state set yet)
                    if (statefulDevice.PowerOn ?? true)
                        statefulDevice.SwitchOff($"RULE: {rule.Name}, CONDITION CHANGE: {condition.Id} ({condition.LastSensorValue})");
                }
            }
        }

        //private void RuleRunner()
        //{
        //    while (true)
        //    {
        //        var sensors = _controller.MqttDevices.Where(d => d is ISensorDevice<ISensorData>).Select(d => d as ISensorDevice<ISensorData>);

        //        foreach (var rule in RuleConfig.Rules)
        //        {

        //            // get the device
        //            //var s = _controller.MqttDevices.Where(d => d.Id == rule.Switch && d is IStatefulDevice).First() as IStatefulDevice;

        //            //var on = false;

        //            //try
        //            //{
        //            //    on = sensors.Any(s => s.TestCondition(rule.Condition));
        //            //}
        //            //catch (Exception err)
        //            //{
        //            //    Console.WriteLine(err.Message);
        //            //}

        //            //if (on)
        //            //{
        //            //    if (!s.PowerOn)
        //            //        s.SwitchOn($"RULE: {rule.Name}", rule.FlipFlop);
        //            //}
        //            //else
        //            //{
        //            //    if (s.PowerOn)
        //            //        s.SwitchOff($"RULE: {rule.Name}");
        //            //}


        //            //try
        //            //{

        //            //}
        //            //catch (Exception err) {
        //            //    _controller.RuleLog.Error($"RuleRunner :: Error in rule {rule.Name} - {err.Message}", err);
        //            //}
        //        }

        //        // wait 5 seconds before next loop - this will give the switches time to change state and report back -- needs to be monitored and refined
        //        Thread.Sleep(5000);
        //    }
        //}

        public void OnDeviceStateChanged(MqttDevice device, StateChangedEventArgs e)
        {
        }

        public void OnDeviceSensorDataChanged(ISensorDevice<ISensorData> device, Dictionary<string, object> allSensorValues)
        {
            foreach (var condition in ConditionConfig.Conditions.Where(c => c.Device == device.Id))
                condition.CheckCondition(device, allSensorValues);
        }
    }
}
