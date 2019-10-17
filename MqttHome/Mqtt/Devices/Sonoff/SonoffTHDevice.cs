using System;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MQTTnet;
using Newtonsoft.Json;

namespace InfluxDbLoader.Mqtt
{
    public class SonoffTHDevice : MqttDevice
    {
        public SonoffTHDevice(string id)
            : base(id)
        {
            SetPowerStateOn = new MqttCommand(id, $"cmnd/{id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(id, $"cmnd/{id}/Power", "OFF");
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.SonoffTH;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));
            PowerOn = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
        }

        public override void ParseSensorPayload(MqttApplicationMessage message)
        {
            SensorData = JsonConvert.DeserializeObject<SonoffTHSensorData>(Encoding.UTF8.GetString(message.Payload));
        }
    }
}
