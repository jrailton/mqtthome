using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        /// <param name="password">The password of tasmotas if already set</param>
        public static List<DiscoveryResult> DiscoverDevices(string ipv4base, string password = null)
        {
            return CheckHosts(string.Join(".", ipv4base.Split(".").Take(3)), password);
        }

        public async Task<TasmotaConfig> GetDeviceConfig(string ipv4address, string password)
        {
            // setup user/pass if specified
            var userPass = string.Empty;
            if (!string.IsNullOrEmpty(password))
                userPass = $"&user=admin&password={Uri.EscapeDataString(password)}";

            var res = await GetHttpResponse($"http://{ipv4address}/cm?cmnd=Status%200{userPass}");

            if (res.IsSuccessStatusCode)
            {
                var responseText = await ReadHttpResponseContent(res);
                return JsonConvert.DeserializeObject<TasmotaConfig>(responseText);
            }

            throw new Exception($"Request failed with response HTTP status code {res.StatusCode}");
        }

        private static List<DiscoveryResult> CheckHosts(string localIpPrefix, string pass = null)
        {
            var taskList = new List<Task<DiscoveryResult>>();
            for (var i = 1; i < 256; i++)
                taskList.Add(CheckHost($"{localIpPrefix}.{i}", pass));

            Task.WaitAll(taskList.ToArray());

            return taskList.Select(t => t.Result).Where(r => r.Available).ToList();
        }

        private static async Task<DiscoveryResult> CheckHost(string ipAddress, string pass)
        {
            var output = new DiscoveryResult(ipAddress);
            var pinger = new Ping();
            for (var attempt = 1; attempt < 5; attempt++)
            {
                // send ping to determine availability
                var pingResponse = await pinger.SendPingAsync(ipAddress, 1000);
                if (pingResponse.Status == IPStatus.Success)
                {
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
                            var responseText = await ReadHttpResponseContent(res);

                            try
                            {
                                var tasmotaResponse = JsonConvert.DeserializeObject<TasmotaResponse>(responseText);

                                output.IsTasmota = true;
                                output.TasmotaResponse = tasmotaResponse;
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            output.Errors = $"Response code {res.StatusCode}";
                        }
                    }
                    catch (Exception err)
                    {
                        output.Errors = err.Message;
                    }
                }
            }
            return output;
        }

        private static async Task<string> ReadHttpResponseContent(HttpResponseMessage res)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    return await res.Content.ReadAsStringAsync();
                }
                catch
                {
                    if (attempt == 4)
                        throw;
                }
                attempt++;
            }
        }

        private static async Task<HttpResponseMessage> GetHttpResponse(string url)
        {
            int attempt = 0;
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
                    if (attempt == 4)
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
