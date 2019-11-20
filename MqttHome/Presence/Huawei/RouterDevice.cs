using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MqttHome.Presence.Huawei
{
    public class RouterDevice
    {
        public RouterDevice(string input)
        {
            // 0 DOMAIN                                          1 IP                 2 MAC                              3 PORT  4 IP TYPE 5 DEVTYPE  6 DEV STATUS 7 PORT TYPE 8 TIME    9 HOSTNAME   10 IPV4 ENABLED 11 IPV6ENABLED 12 DEV TYPE
            //"InternetGatewayDevice.LANDevice.1.X_HW_UserDev.2","10\x2e0\x2e0\x2e59","80\x3a5e\x3a4f\x3a6c\x3ac7\x3a19","SSID1","DHCP",   "",        "Online",    "WIFI",     "0\x3a34","beaglebone","1",            "1",              "0"
            var fields = input.Split(',').Select(s => Unescape(s)).ToArray();
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
            IPV4 = fields[10].Equals("1");
            IPV6 = fields[11].Equals("1");
        }

        private string Unescape(string input)
        {
            return Regex.Unescape(input);
            //return Regex.Replace(input, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
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
