using System;
using System.Collections.Generic;

namespace MqttHome.Mqtt
{
    public interface IMqttDevice : IDevice
    {
        public DateTime? LastCommunication { get; }

        public List<string> AllTopics { get; }

        // commands
        public MqttCommand RebootCommand { get; }
    }
}
