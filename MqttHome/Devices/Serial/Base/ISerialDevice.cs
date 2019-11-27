using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Devices.Serial.Base
{
    public interface ISerialDevice : IDevice
    {
        public DateTime? LastCommunication { get; }
    }
}
