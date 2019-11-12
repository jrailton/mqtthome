using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.Devices.Sonoff
{
    public class SonoffBasicDevice : SonoffGenericSwitchDevice
    {
        public SonoffBasicDevice(MqttHomeController controller, string id, string friendlyName, params string[] config) : base(controller, id, friendlyName, MqttDeviceType.SonoffBasic, config) { }
    }
}
