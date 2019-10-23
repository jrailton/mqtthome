using System;
using System.Collections.Generic;
using System.Text;
using InfluxDbLoader.Mqtt;

namespace MqttHome.Mqtt.Devices
{
    public class MqttDeviceState
    {
        public MqttDevice Device;
        public eMqttDeviceStateChangeType Type;
        public SensorData SensorData;
        public bool PowerOn;
    }

    public enum eMqttDeviceStateChangeType
    {
        Power = 1,
        SensorData = 2
    }
}
