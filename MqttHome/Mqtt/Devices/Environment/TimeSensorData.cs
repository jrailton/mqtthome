using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeSensorData : SensorData
    {
        public override void Update(MqttApplicationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
