using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MqttHome.Presence.Huawei
{
    public class HuaweiHG8245
    {
        private string _host = "10.0.0.1";
        private string _username = "telecomadmin";
        private string _password = "lt16sXtu";
        private IEnumerable<string> _cookies;

        public HuaweiHG8245()
        {
            _password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_password));
        }

        private class PostResponse
        {
            public IEnumerable<string> Cookies;
            public string Content;
        }

        private PostResponse Send(HttpMethod method, string url, IEnumerable<KeyValuePair<string, string>> body = null, IEnumerable<string> cookies = null)
        {
            var uri = new Uri(url);

            using (var handler = new HttpClientHandler() { UseCookies = false/*, Proxy = new WebProxy("http://localhost:8888")*/ })
            {
                using (var client = new HttpClient(handler) { BaseAddress = uri })
                {
                    var request = new HttpRequestMessage(method, uri);

                    if (cookies != null)
                        request.Headers.Add("Cookie", string.Join("; ", cookies));

                    if (body != null)
                        request.Content = new FormUrlEncodedContent(body);

                    var result = client.SendAsync(request).Result;
                    result.EnsureSuccessStatusCode();

                    var response = new PostResponse
                    {
                        Content = result.Content.ReadAsStringAsync().Result
                    };

                    IEnumerable<string> returnedCookies;
                    if (result.Headers.TryGetValues("Set-cookie", out returnedCookies))
                        response.Cookies = new[] { returnedCookies.First().Split(';')[0] };

                    return response;
                }
            }
        }

        public void Login()
        {
            var randCount = Send(HttpMethod.Post, $"http://{_host}/asp/GetRandCount.asp");

            // log in to router
            var loginResponse = Send(HttpMethod.Post,
            $"http://{_host}/login.cgi", new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("UserName", _username),
                new KeyValuePair<string, string>("PassWord", _password),
                new KeyValuePair<string, string>("x.X_HW_Token", randCount.Content)
            },
            new[] { "Cookie=body:Language:english:id=-1" });

            // set cookies from login
            _cookies = loginResponse.Cookies;

            if ((_cookies?.Count() ?? 0) == 0)
                throw new Exception("Login failed");
        }

        public string GetDevicesPage()
        {
            // requesting lan user info update
            // this request is needed or else some devices' state won't be updated
            Send(HttpMethod.Get, $"http://{_host}/html/bbsp/common/lanuserinfo.asp", null, _cookies);

            // Requesting lan user info data
            return Send(HttpMethod.Get, $"http://{_host}/html/bbsp/common/GetLanUserDevInfo.asp", null, _cookies).Content;
        }

        public List<RouterDevice> GetDevices()
        {
            Login();

            MatchCollection matches = Regex.Matches(GetDevicesPage(), @"new USERDevice\(([^\)]*)\)[,]*");
            var output = new List<RouterDevice>();

            foreach (Match match in matches)
                output.Add(new RouterDevice(match.Groups[1].Value));

            return output;
        }
    }

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
