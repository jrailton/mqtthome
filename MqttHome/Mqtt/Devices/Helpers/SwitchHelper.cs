using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    class SwitchHelper
    {
        private IStatefulDevice _device;

        public SwitchHelper(IStatefulDevice device) {
            _device = device;
        }

        public void SwitchOff(string reason)
        {
            _device.Controller.DeviceLog.Info($"SwitchOff :: Reason - {reason}");
            _device.SetPowerStateOff.Execute();
        }

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            // default to 15 seconds if null
            flipFlopSeconds = flipFlopSeconds ?? 15;

            _device.Controller.DeviceLog.Info($"SwitchOn :: Reason - {reason}");

            // prevent flipflop
            if (_device.PowerOffTime.HasValue && _device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value) > DateTime.Now)
            {
                var error = $"Flipflop prevention. Need to wait until {_device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value).ToString("HH:mm:ss")}";
                _device.Controller.DeviceLog.Warn($"SwitchOn :: Reason - {reason} :: Aborted - {error}");
                throw new Exception(error);
            }
            else
            {
                _device.SetPowerStateOn.Execute();
            }
        }
    }
}
