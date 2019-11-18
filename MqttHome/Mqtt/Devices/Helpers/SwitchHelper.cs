using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    class SwitchHelper
    {
        private IStatefulDevice _device;

        public SwitchHelper(IStatefulDevice device)
        {
            _device = device;
        }

        public void SwitchOff(string reason)
        {
            var logIdentity = $"{_device.Id} :: SwitchOff";
            try
            {
                _device.Controller.DeviceLog.Info($"{logIdentity} :: Reason - {reason}");
                _device.SetPowerStateOff.Execute();
            }
            catch (Exception err)
            {
                _device.Controller.DeviceLog.Error($"{logIdentity} - Failed.", err);
            }
        }

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            var logIdentity = $"{_device.Id} :: SwitchOn";
            try
            {

                // default to 15 seconds if null
                flipFlopSeconds = flipFlopSeconds ?? 15;

                _device.Controller.DeviceLog.Info($"{logIdentity} :: Reason - {reason}");

                // prevent flipflop
                if (_device.PowerOffTime.HasValue && _device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value) > DateTime.Now)
                {
                    var error = $"Flipflop prevention. Need to wait until {_device.PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value).ToString("HH:mm:ss")}";
                    _device.Controller.DeviceLog.Warn($"{logIdentity} :: Reason - {reason} :: Aborted - {error}");
                }
                else
                {
                    _device.SetPowerStateOn.Execute();
                }
            }
            catch (Exception err)
            {
                _device.Controller.DeviceLog.Error($"{logIdentity} - Failed.", err);
            }

        }
    }
}
