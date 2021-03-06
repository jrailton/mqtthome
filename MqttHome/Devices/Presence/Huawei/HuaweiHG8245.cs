﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MqttHome.Presence.Huawei
{
    public class HuaweiHG8245 : PresenceDevice
    {
        private string _host;
        private string _username;
        private string _password;
        private IEnumerable<string> _cookies;
        private Timer _timer;
        private List<RouterDevice> _devices;

        public HuaweiHG8245(MqttHomeController controller, Config.Device config) : base(controller, config)
        {
            _host = config.Parameters[0];
            _username = config.Parameters[1];
            _password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(config.Parameters[2]));

            //check every 20 seconds, starting immediately
            _timer = new Timer((state) =>
            {
                UpdatePeople();
            }, null, 0, 20000);
        }

        private void UpdatePeople() 
        {
            try
            {
                // get list of attached devices
                _devices = GetDevices();

                // check list of People
                foreach (var person in Controller.People)
                {
                    var device = _devices.SingleOrDefault(d => d.MacAddress == person.MacAddress);

                    // if device is found and online, then they are present -- default to not-present
                    var present = device?.Status.Equals("Online") ?? false;

                    // if present, person state will immediately change but if not present, state will only change after 5 minutes (because wifi devices disconnect/reconnect often)
                    if (present)
                    {
                        person.LastSeen = DateTime.Now;
                    }
                    else if (DateTime.Now.Subtract(person.LastSeen ?? DateTime.MinValue).TotalSeconds <= 300)
                    {
                        // the person is not present, but was last seen up to 5 minutes ago, so dont assume presence has changed yet
                        present = true;

                        // N.B. last seen date will begin to age so after 5 minutes, presence will be changed to false
                    }

                    // if presence changed, raise event
                    if (person.Present != present)
                    {
                        person.Present = present;
                        person.PresenceChanged = DateTime.Now;

                        OnPresenceChanged(this, person);
                    }
                }
            }
            catch (Exception err) {
                Controller.DeviceLog.Error($"{DeviceClass}, {DeviceType}, {Id}, {FriendlyName} :: UpdatePeople - Failed", err);
            }
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
