using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt
{
    public class MqttCommand
    {
        public MqttCommand(MqttHomeController controller, string deviceId, string topic, string payload) : this(controller, deviceId, topic, Encoding.UTF8.GetBytes(payload))
        {
        }

        public MqttCommand(MqttHomeController controller, string deviceId, string topic, byte[] payload)
        {
            Topic = topic;
            Payload = payload;
            DeviceId = deviceId;
            _controller = controller;
        }

        private MqttHomeController _controller;
        public string DeviceId { get; set; }
        public string Topic { get; set; }
        public byte[] Payload { get; set; }

        public void Execute(MqttCommunicator communicator = null)
        {
            if (communicator != null)
            {
                communicator.PublishCommand(this);
            }
            else
            {
                // publish the command on all brokers
                _controller.MqttCommunicators.ForEach(c => c.PublishCommand(this));
            }
        }
    }
}
