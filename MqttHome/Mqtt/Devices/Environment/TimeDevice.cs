using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeDevice : MqttSensorDevice<TimeSensorData>
    {
        public TimeDevice(MqttHomeController controller, string id) : base(controller, id, MqttDeviceType.TimeSensor) { }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.Unknown;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Sensor;
    }
}
