using System;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffGenericSwitchDevice : MqttDevice
    {
        public SonoffGenericSwitchDevice(MqttHomeController controller, string id, MqttDeviceType type)
            : base(controller, id)
        {
            DeviceType = type;
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.Unknown;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));
            PowerOn = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
        }

        public override void ParseSensorPayload(MqttApplicationMessage message)
        {
            // generic switches dont have sensor payloads
            throw new NotImplementedException();
        }
    }
}
