using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.Devices.Sonoff
{
    public class SonoffS26Device : SonoffGenericSwitchDevice
    {
        public SonoffS26Device(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.SonoffS26, config) { }
    }
}
