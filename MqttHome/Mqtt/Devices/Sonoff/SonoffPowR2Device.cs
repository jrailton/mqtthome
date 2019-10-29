using System.Text;
using InfluxDB.LineProtocol.Payload;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffPowR2Device : MqttSensorDevice<SonoffPowR2SensorData>, IStatefulDevice
    {
        public SonoffPowR2Device(MqttHomeController controller, string id)
            : base(controller, id)
        {
            SetPowerStateOn = new MqttCommand(controller, id, $"cmnd/{id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(controller, id, $"cmnd/{id}/Power", "OFF");
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.SonoffPowR2;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;

        public void ParseStatePayload(MqttApplicationMessage message)
        {
            PowerOn = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload)).PowerOn;
        }
    }
}