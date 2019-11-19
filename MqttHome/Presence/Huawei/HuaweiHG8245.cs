using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

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
            var baseAddress = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}";

            using (var handler = new HttpClientHandler() { UseCookies = false, Proxy = new WebProxy("http://localhost:8888") })
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

        public void Login()
        {
            var randCount = Send(HttpMethod.Post, $"http://{_host}/asp/GetRandCount.asp");

            //_LOGGER.debug("Logging in")
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

        public string GetDevices()
        {
            //_LOGGER.debug("Requesting lan user info update")
            //# this request is needed or else some devices' state won't be updated
            Send(HttpMethod.Get, $"http://{_host}/html/bbsp/common/lanuserinfo.asp", null, _cookies);

            //_LOGGER.debug("Requesting lan user info data")
            return Send(HttpMethod.Get, $"http://{_host}/html/bbsp/common/GetLanUserDevInfo.asp", null, _cookies).Content;
        }
    }
}
