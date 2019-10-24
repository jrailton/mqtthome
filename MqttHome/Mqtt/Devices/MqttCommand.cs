using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt
{
    public class MqttCommand
    {
        public MqttCommand(MqttHomeController controller, string deviceId, string topic, string payload) : this(controller, deviceId, topic, Encoding.UTF8.GetBytes(payload))
        {
        }

        public MqttCommand(MqttHomeController controller, string deviceId, string topic, IEnumerable<byte> payload)
        {
            Topic = topic;
            Payload = payload;
            DeviceId = deviceId;
            _controller = controller;
        }

        private MqttHomeController _controller;
        public string DeviceId { get; set; }
        public string Topic { get; set; }
        public IEnumerable<byte> Payload { get; set; }

        public void Execute()
        {
            _controller.MqttCommunicator.PublishCommand(this);
        }
    }
}
