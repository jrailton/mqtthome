using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MqttHomeWeb.Models
{
    public class GoogleOAuthProvider
    {
        private string _token;
        private DateTime _tokenExpires;

        public class GoogleTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }

        public class GoogleUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string email { get; set; }
            public string token { get; set; }
            public DateTime tokenExpires { get; set; }
        }

        public GoogleOAuthProvider(string code)
        {
            var client = new WebClient();

            var queryString = new NameValueCollection
                {
                    {"client_id", Program.Config["GoogleAppId"]},
                    {"client_secret", Program.Config["GoogleAppSecret"]},
                    {"redirect_uri", Program.Config["BaseUrl"] + "/account/oauthresponse"},
                    {"code", code},
                    {"grant_type", "authorization_code"}
                };

            var data = client.UploadValues("https://accounts.google.com/o/oauth2/token", "POST", queryString);
            var token = JsonConvert.DeserializeObject<GoogleTokenResponse>(System.Text.Encoding.UTF8.GetString(data));
            _token = token.access_token;
            _tokenExpires = DateTime.Now.AddSeconds(token.expires_in);
        }

        public GoogleUser GetUserData()
        {
            var client = new WebClient
            {
                QueryString = new NameValueCollection
                        {
                            { "access_token", _token }
                        }
            };

            var json = client.DownloadString("https://www.googleapis.com/oauth2/v1/userinfo");

            var user = JsonConvert.DeserializeObject<GoogleUser>(json);

            if (string.IsNullOrEmpty(user.email))
                throw new Exception("Google didn't want us to see your e-mail address, so we can't log you in! Sorry :(");

            user.token = _token;
            user.tokenExpires = _tokenExpires;

            return user;
        }
    }
}
