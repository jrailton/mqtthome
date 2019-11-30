using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.Devices.Sonoff
{
    public class SonoffBasicDevice : SonoffGenericSwitchDevice
    {
        public SonoffBasicDevice(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.SonoffBasic, config) { }
    }
}
