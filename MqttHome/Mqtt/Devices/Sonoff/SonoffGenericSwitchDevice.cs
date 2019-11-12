using System;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffGenericSwitchDevice : MqttStatefulDevice
    {
        private SwitchHelper _switchHelper;
        public SonoffGenericSwitchDevice(MqttHomeController controller, string id, string friendlyName, MqttDeviceType type, params string[] config) : base(controller, id, friendlyName, type, config)
        {
            _switchHelper = new SwitchHelper(this);
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.Unknown;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));

            var newState = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
            if (PowerOn != newState)
                PowerOn = newState;
        }
    }
}
