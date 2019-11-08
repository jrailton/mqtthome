using MqttHome.Devices.Config;
using System.Collections.Generic;

namespace MqttHome
{
    public class DeviceConfig
    {
        public List<Device> Devices { get; set; } = new List<Device>();
    }
}
