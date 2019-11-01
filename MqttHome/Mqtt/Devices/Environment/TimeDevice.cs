using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeDevice : MqttSensorDevice<TimeSensorData>
    {
        public TimeDevice(MqttHomeController controller, string id) : base(controller, id) { }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.Unknown;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Sensor;

        public override bool IsSubscribedToSensorTopic(string topic)
        {
            return false;
        }

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
