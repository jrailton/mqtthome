using System;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffPowR2Device : MqttStatefulSensorDevice<SonoffPowR2SensorData>
    {
        public SonoffPowR2Device(MqttHomeController controller, string id)
            : base(controller, id, MqttDeviceType.SonoffPowR2)
        {
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.SonoffPowR2;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;
    }
}