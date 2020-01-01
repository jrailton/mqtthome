using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MqttHomeWeb.Models.Tasmota;
using Newtonsoft.Json;

namespace MqttHomeWeb.Repositories
{
    public class Tasmota
    {
        /// <summary>
        /// Returns a list of available devices and settings if they are Tasmota devices
        /// </summary>
        /// <param name="ipv4base">The ip address of the network to be scanned e.g. 10.0.0.x</param>
        /// <param name="password">The password of Tasmotas if already set</param>
        public static List<DiscoveryResult> DiscoverDevices(string ipv4base, string password = null, Action<string> logger = null)
        {
            return CheckHosts(string.Join(".", ipv4base.Split(".").Take(3)), password, logger);
        }

        public static async Task<TasmotaConfig> GetDeviceConfig(string ipv4address, string password)
        {
            // setup user/pass if specified
            var userPass = string.Empty;
            if (!string.IsNullOrEmpty(password))
                userPass = $"&user=admin&password={Uri.EscapeDataString(password)}";

            var res = await GetHttpResponse($"http://{ipv4address}/cm?cmnd=Status%200{userPass}");

            if (res.IsSuccessStatusCode)
            {
                var responseText = await ReadHttpResponseContent(res);
                var output = JsonConvert.DeserializeObject<TasmotaConfig>(responseText);

                if (!string.IsNullOrEmpty(output.WARNING))
                    throw new Exception(output.WARNING);

                return output;
            }

            throw new Exception($"Request failed with response HTTP status code {res.StatusCode}");
        }

        /// <summary>
        /// Returns new ipaddress and password (because config MAY have specified an updated ipaddress and/or password)
        /// </summary>
        public static async Task<Tuple<string, string, string>> SetDeviceConfig(string ipv4address, string password, Dictionary<string, string> config) {
            //Backlog mqtthost <yourhost>; mqttuser <user>; mqttpassword <password>; topic <customtopic>; setoption53 1; powerretain on
            //When using web requests (You have to encode "space" as '%20' and ";" as '%3B')
            //http://<ip>/cm?user=admin&password=joker&cmnd=Backlog%20Power%20Toggle%3BPower1%20ff

            var backlog = "";

            // remove blank and irrelevant entries
            foreach (var key in config.Keys.Where(k => !string.IsNullOrEmpty(config[k])))
            {
                backlog += $"{key} {config[key]};";
            }

            backlog = WebUtility.UrlEncode(backlog).Replace("+", "%20");

            var command = $"http://{ipv4address}/cm?{(string.IsNullOrEmpty(password) ? "" : $"user=admin&password={password}&")}cmnd=Backlog%20{backlog}";

            var response = await GetHttpResponse(command, 1);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Response code from device was {response.StatusCode} ({response.ReasonPhrase})");

            return new Tuple<string, string, string>(ipv4address, config.ContainsKey("WebPassword") ? config["WebPassword"] : password, responseText);
        }

        private static List<DiscoveryResult> CheckHosts(string localIpPrefix, string pass = null, Action<string> logger = null)
        {
            var taskList = new List<Task<DiscoveryResult>>();
            for (var i = 1; i < 256; i++)
                taskList.Add(CheckHost($"{localIpPrefix}.{i}", pass, logger));

            Task.WaitAll(taskList.ToArray());

            return taskList.Select(t => t.Result).Where(r => r.Available).ToList();
        }

        private static async Task<DiscoveryResult> CheckHost(string ipAddress, string pass, Action<string> logger = null)
        {
            var output = new DiscoveryResult(ipAddress);
            var pinger = new Ping();
            for (var attempt = 1; attempt < 5; attempt++)
            {
                // send ping to determine availability
                var pingResponse = await pinger.SendPingAsync(ipAddress, 1000);
                if (pingResponse.Status == IPStatus.Success)
                {
                    logger($"{ipAddress} is online. Checking Tasmota...");

                    output.Available = true;

                    // setup user/pass if specified
                    var userPass = string.Empty;
                    if (!string.IsNullOrEmpty(pass))
                        userPass = $"&user=admin&password={Uri.EscapeDataString(pass)}";

                    try
                    {
                        var res = await GetHttpResponse($"http://{ipAddress}/cm?cmnd=FriendlyName{userPass}");

                        if (res.IsSuccessStatusCode)
                        {
                            // this cluster-fuck below is because simply sending a STATUS 0 command to tasmota results in a 30% success ratio
                            // so we need to ask for values one at a time AND to make matters worse, asking for stuff like FriendlyName results in a response like FriendlyName1 = xxxx
                            // anyhoo... it works

                            var responseText = await ReadHttpResponseContent(res);

                            try
                            {
                                var config = JsonConvert.DeserializeObject<TasmotaResponse>(responseText);

                                output.IsTasmota = true;
                                output.Config = config;

                                logger($"{ipAddress}{(string.IsNullOrEmpty(config.FriendlyName1) ? "" : $" {config.FriendlyName1}")} is running Tasmota.");

                                // see if we can get the topic as well
                                res = await GetHttpResponse($"http://{ipAddress}/cm?cmnd=Topic{userPass}");
                                responseText = await ReadHttpResponseContent(res);
                                var woft = JsonConvert.DeserializeObject<TasmotaResponse>(responseText);
                                config.Topic = woft.Topic;

                                // successful, return early
                                return output;
                            }
                            catch
                            {
                                logger($"{ipAddress} did not return a recognized response ({WebUtility.HtmlEncode(responseText.Substring(0, responseText.Length > 200 ? responseText.Length : 200))})");

                                // guaranteed not tasmota, return early
                                return output;
                            }
                        }
                        else
                        {
                            output.Errors = $"Response code {res.StatusCode}";
                            logger($"{ipAddress} responded with {res.StatusCode} ({res.ReasonPhrase})");

                            // guaranteed not tasmota, return early
                            return output;
                        }
                    }
                    catch (Exception err)
                    {
                        output.Errors = err.Message;
                        logger($"{ipAddress} responded with error {err.Message}");

                        // check for a non-retryable error message
                        if (err.Message == "The SSL connection could not be established, see inner exception." || err.Message == "No connection could be made because the target machine actively refused it.")
                            return output;
                    }
                }
            }
            return output;
        }

        private static async Task<string> ReadHttpResponseContent(HttpResponseMessage res, int attempts = 1)
        {
            int attempt = 1;
            while (true)
            {
                try
                {
                    return await res.Content.ReadAsStringAsync();
                }
                catch
                {
                    if (attempt >= attempts)
                        throw;
                }
                attempt++;
            }
        }

        public static async Task<HttpResponseMessage> GetHttpResponse(string url, int attempts = 1)
        {
            int attempt = 1;
            while (true)
            {
                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Get, url);

                try
                {
                    return await client.SendAsync(req);
                }
                catch
                {
                    if (attempt >= attempts)
                        throw;
                }
                attempt++;
            }
        }

        public static Dictionary<int, string> Modules = new Dictionary<int, string>
            {
                {1, "Sonoff Basic"},
                {2, "Sonoff RF"},
                {3, "Sonoff SV"},
                {4, "Sonoff TH"},
                {5, "Sonoff Dual"},
                {6, "Sonoff POW"},
                {7, "Sonoff 4Ch"},
                {8, "Sonoff S2X"},
                {9, "Slampher"},
                {10, "Sonoff Touch"},
                {11, "Sonoff LED"},
                {12, "1 Channel"},
                {13, "4 Channel"},
                {14, "Motor C/AC"},
                {15, "ElectroDragon"},
                {16, "EXS Relay(s)"},
                {17, "WION"},
                {18, "Generic"},
                {19, "Sonoff Dev"},
                {20, "H801"},
                {21, "Sonoff SC"},
                {22, "Sonoff BN-SZ"},
                {23, "Sonoff 4Ch Pro"},
                {24, "Huafan SS"},
                {25, "Sonoff Bridge"},
                {26, "Sonoff B1"},
                {27, "Ailight"},
                {28, "Sonoff T1 1Ch"},
                {29, "Sonoff T1 2Ch"},
                {30, "Sonoff T1 3Ch"},
                {31, "Supla Espablo"},
                {32, "Witty Cloud"},
                {33, "Yunshan Relay"},
                {34, "MagicHome"},
                {35, "Luani HVIO"},
                {36, "KMC 70011"},
                {37, "Arilux LC01"},
                {38, "Arilux LC11"},
                {39, "Sonoff Dual R2"},
                {40, "Arilux LC06"},
                {41, "Sonoff S31"},
                {42, "Zengge WF017"},
                {43, "Sonoff Pow R2"},
                {44, "Sonoff IFan02"},
                {45, "Blitzwolf SHP"},
                {46, "Shelly 1"},
                {47, "Shelly 2"},
                {48, "Xiaomi Philips"},
                {49, "Neo Coolcam"},
                {50, "ESP SwitCh"},
                {51, "Obi Socket"},
                {52, "Teckin"},
                {53, "APLIC WDP303075"},
                {54, "TuyaMCU"},
                {55, "Gosund SP1 v23"},
                {56, "Armtronix Dimmers"},
                {57, "SK03 Outdoor(Tuya)"},
                {58, "PS-16-DZ"},
                {59, "Teckin US"},
                {60, "Manzoku Strip(EU 4)"},
                {61, "Obi Socket 2"},
                {62, "YTF LR Bridge"},
                {63, "Digoo DG-SP202"},
                {64, "KA10"},
                {65, "Luminea ZX2820"},
                {66, "Mi Desk Lamp"},
                {67, "SP10"},
                {68, "WAGA CHCZ02MB"},
                {69, "SYF05"},
                {70, "Sonoff L1"},
                {71, "Sonoff iFan03"},
                {72, "EX-Store Dimmer"}
            };

        public static List<SelectListItem> PowerOnStates(int? selected) {
            return new List<SelectListItem> {
                new SelectListItem{ Text = "Off", Value = "0", Selected = (selected == 0) },
                new SelectListItem{ Text = "On", Value = "1", Selected = (selected == 1) },
                new SelectListItem{ Text = "Toggle", Value = "2", Selected = (selected == 2) },
                new SelectListItem{ Text = "Remember (default)", Value = "3", Selected = (selected == 3) },
            };
        }

        public static List<string> Configs = new List<string>{
            "Timezone 2",
            "Longitude",
            "Latitude",
            "TelePeriod 10",
            "FriendlyName",
            "WebPassword", // webui password
            "MqttHost", // host IP
            "MqttUser", // host usernmae
            "MqttPassword", // host password
            "Topic powr2_5", // client name
            "FullTopic %25prefix%25%2F%25topic%25%2F"
        };
    }
}
