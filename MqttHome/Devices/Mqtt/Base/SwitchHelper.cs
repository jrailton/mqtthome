using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    class SwitchHelper
    {
        private ISwitchDevice _device;
        public Dictionary<DateTime, string> StateHistory = new Dictionary<DateTime, string>();

        public SwitchHelper(ISwitchDevice device)
        {
            _device = device;
        }

        public void SwitchOff(string reason)
        {
            var logIdentity = $"{_device.Id} :: SwitchOff";
            try
            {
                AddStateHistory($"OFF: {reason}");

                _device.Controller.RuleLog.Info($"{logIdentity} :: Reason - {reason}");
                _device.SetPowerStateOff.Execute();
            }
            catch (Exception err)
            {
                AddStateHistory($"(Failed) OFF: {reason}");

                _device.Controller.RuleLog.Error($"{logIdentity} - Failed.", err);
            }
        }

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            var logIdentity = $"{_device.Id} :: SwitchOn";
            try
            {

                // default to 15 seconds if null
                flipFlopSeconds = flipFlopSeconds ?? 15;

                _device.Controller.RuleLog.Info($"{logIdentity} :: Reason - {reason}");

                // prevent flipflop
                if (_device.PowerOffTime.HasValue && _device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value) > DateTime.Now)
                {
                    AddStateHistory($"(FlipFlop) ON: {reason}");

                    var error = $"Flipflop prevention. Need to wait until {_device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value).ToString("HH:mm:ss")}";
                    _device.Controller.RuleLog.Warn($"{logIdentity} :: Reason - {reason} :: Aborted - {error}");
                }
                else
                {
                    AddStateHistory($"ON: {reason}");

                    _device.SetPowerStateOn.Execute();
                }
            }
            catch (Exception err)
            {
                AddStateHistory($"(Failed) ON: {reason}");

                _device.Controller.RuleLog.Error($"{logIdentity} - Failed.", err);
            }

        }

        public void AddStateHistory(string message) {
            StateHistory.Add(DateTime.Now, message);

            // dont let the list grow to more than 20 items
            if (StateHistory.Count > 20)
                StateHistory.Remove(StateHistory.Keys.Last());
        }

        public string StateQuery
        {
            get
            {
                var rules = _device.Controller.RuleEngine.RuleConfig.Rules.Where(r => r.Switch == _device.Id);
                var offRules = rules.Where(r => (r.State ?? false) == false);
                var onRules = rules.Where(r => (r.State ?? true) == true);

                if (!rules.Any())
                    return "No rules governing this switch";

                if (_device.PowerOn ?? true) // if power on (or no state known)
                {
                    if (offRules.Any())
                        return $"Should be OFF because of {string.Join(", ", offRules.Select(r => r.Name))}";

                    return $"ON because of {string.Join(", ", onRules.Select(r => r.Name))}";
                }
                else // if power off (or no state known)
                {
                    if (!offRules.Any())
                        return "Should be ON - possibly overridden";

                    return $"OFF because of {string.Join(", ", offRules.Select(r => r.Name))}";
                }
            }
        }
    }
}
