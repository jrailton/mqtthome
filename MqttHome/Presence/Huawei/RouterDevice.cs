using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MqttHome.Presence.Huawei
{
    public class RouterDevice
    {
        public static implicit operator Person(RouterDevice o) {
            return new Person
            {
                DeviceType = o.DeviceType,
                Domain = o.Domain,
                IpAddress = o.IpAddress,
                IpType = o.IpType,
                IPV4 = o.IPV4,
                IPV6 = o.IPV6,
                MacAddress = o.MacAddress,
                Port = o.Port,
                PortType = o.PortType,
                Status = o.Status,
                Time = o.Time
            };
        }

        public RouterDevice(string input)
        {
            // 0 DOMAIN                                          1 IP                 2 MAC                              3 PORT  4 IP TYPE 5 DEVTYPE  6 DEV STATUS 7 PORT TYPE 8 TIME    9 HOSTNAME   10 IPV4 ENABLED 11 IPV6ENABLED 12 DEV TYPE
            //"InternetGatewayDevice.LANDevice.1.X_HW_UserDev.2","10\x2e0\x2e0\x2e59","80\x3a5e\x3a4f\x3a6c\x3ac7\x3a19","SSID1","DHCP",   "",        "Online",    "WIFI",     "0\x3a34","beaglebone","1",            "1",              "0"
            var fields = input.Split(',').Select(s => Regex.Unescape(s).Replace("\"", "")).ToArray();
            Domain = fields[0];
            IpAddress = fields[1];
            MacAddress = fields[2];
            Port = fields[3];
            IpType = fields[4];
            DeviceType = fields[5];
            Status = fields[6];
            PortType = fields[7];
            Time = fields[8];
            Hostname = fields[9];
            IPV4 = fields[10].Contains("1");
            IPV6 = fields[11].Contains("1");
        }

        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Port { get; set; }
        public string IpType { get; set; }
        public string DeviceType { get; set; }
        public string Status { get; set; }
        public string PortType { get; set; }
        public string Time { get; set; }
        public string Hostname { get; set; }
        public bool IPV4 { get; set; }
        public bool IPV6 { get; set; }
    }
}
