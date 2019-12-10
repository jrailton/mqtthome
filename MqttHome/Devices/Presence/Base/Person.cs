using MqttHome.Presence.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MqttHome.Presence
{
    public class Person
    {
        public string MacAddress { get; set; }
        public string FriendlyName { get; set; }
        public string Id { get; set; }
        public bool Present { get; set; }
        public DateTime? PresenceChanged { get; set; }

        // used to delay "away" state change for X minutes (some devices connect and disconnect often)
        public DateTime? LastSeen { get; set; }

        // these are just here for debugging huawei hg8245 device -- they can be removed
        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string IpType { get; set; }
        public string DeviceType { get; set; }
        public string Status { get; set; }
        public string PortType { get; set; }
        public string Time { get; set; }
        public bool IPV4 { get; set; }
        public bool IPV6 { get; set; }

    }
}
