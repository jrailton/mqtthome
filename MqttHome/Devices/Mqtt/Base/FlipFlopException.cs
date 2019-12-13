using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Devices.Mqtt.Base
{
    public class FlipFlopException : Exception
    {
        public DateTime FlipFlopTimeout;

        public FlipFlopException(DateTime until) {
            FlipFlopTimeout = until;
        }
    }
}
