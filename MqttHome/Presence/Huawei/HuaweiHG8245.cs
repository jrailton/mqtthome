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
    public class HuaweiHG8245 : PresenceDevice
    {
        private string _host = "10.0.0.1";
        private string _username = "telecomadmin";
        private string _password = "lt16sXtu";
        private IEnumerable<string> _cookies;

        public override List<Person> People => throw new NotImplementedException();

        public HuaweiHG8245(MqttHomeController controller, string id, string friendlyName, params string[] config) : base(controller, id, friendlyName, config)
        {
            _host = config[0];
            _username = config[1];
            _password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(config[2]));
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
}
