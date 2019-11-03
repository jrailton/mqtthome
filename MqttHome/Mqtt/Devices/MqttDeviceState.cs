using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.Devices
{
    public class MqttDeviceState
    {
        public MqttDevice Device;
        public bool PowerOn;
    }

    public class MqttSensorState
    {
        public MqttDevice Device;
        public SensorData SensorData;
    }
}
