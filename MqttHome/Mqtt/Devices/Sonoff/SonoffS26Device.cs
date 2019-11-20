using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.Devices.Sonoff
{
    public class SonoffS26Device : SonoffGenericSwitchDevice
    {
        public SonoffS26Device(MqttHomeController controller, string id, string friendlyName, params string[] config) : base(controller, id, friendlyName, DeviceType.SonoffS26, config) { }
    }
}
