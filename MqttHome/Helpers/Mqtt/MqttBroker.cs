using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Mqtt.BrokerCommunicator
{
    public class MqttBroker
    {
        public string Name { get; set; } = "Unnamed Broker";
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}
