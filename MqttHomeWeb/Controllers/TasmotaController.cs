using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Config;
using MqttHomeWeb.Repositories;
using Newtonsoft.Json;

namespace MqttHomeWeb.Controllers
{
    public class TasmotaController : Controller
    {
        private bool _logInit;
        private string _logSetup = @"
<style type='text/css'>
body{ 
    font-family: Arial; 
    font-size: 12px; 
}
</style>

<script type='text/javascript'>
    var autoscroll = setInterval(function(){
        window.scrollTo(0,document.body.scrollHeight);    
    }, 200);
</script>";
        private object _loggerLock = new object();

        public IActionResult Index()
        {
            return View();
        }

        public void Discover(string ipaddress, string password)
        {
            Log($"Starting IP range scan on {ipaddress}...");

            var devices = Tasmota.DiscoverDevices(ipaddress, password, Log);

            Log($"Completed IP range scan. Found {devices.Count} devices ({devices.Count(d => d.IsTasmota)} of which are Tasmota)");

            var tasmotas = devices.Where(d => d.IsTasmota);
            foreach (var device in tasmotas)
            {
                var toLog = @$"
{(string.IsNullOrEmpty(device.Config.WARNING) ? "" : $@"<span style='color:red;'>{device.Config.WARNING}</span>")}
<label>
    <input type='checkbox' value='{device.IpAddress}' class='ipaddress' /> 
    {device.Config.FriendlyName1} ({device.IpAddress})
</label>";

                // if there was a warning (likely invalid user/pass) then show a link directly to the device, otherwise show a link to open 
                // the mqtthome web configuration page
                if (string.IsNullOrEmpty(device.Config.WARNING)){
                    toLog += $" <a href='/tasmota/view?ipaddress={device.IpAddress}&password={password}' target='_blank'>[View]</a>";

                    if (!string.IsNullOrEmpty(device.Config.Topic) && !Program.MqttHomeController.MqttDevices.Any(d => d.Id == device.Config.Topic))
                        toLog += $" <a href='/device/add?id={device.Config.Topic}&friendlyname={device.Config.FriendlyName1}' target='_blank'>[Add New Device]</a>";
                }
                else {
                    toLog += $"<a href='http://{device.IpAddress}' target='_blank'>[View]</a>";
                }

                Log(toLog);
            }

            Log(@"
<script type='text/javascript'>
    window.scrollTo(0,document.body.scrollHeight);
    clearInterval(autoscroll);

    function GetIpAddresses(){
        var os = document.getElementsByClassName('ipaddress'),
            output = '';

        for(var i = 0; i < os.length; i++){
            if (os[i].checked)
                output += (output.length > 0 ? ',' : '') + os[i].value;
        }
        
        return output;
    }
</script>");
        }

        public IActionResult View(string ipaddress, string password)
        {

            try
            {
                ViewBag.IPAddress = ipaddress;
                ViewBag.Password = password;

                return View(Tasmota.GetDeviceConfig(ipaddress, password).Result);
            }
            catch (Exception err)
            {
                TempData["danger"] = $"Failed to read device configuration - {err.Message}";
                return View();
            }
        }

        private void Log(string description)
        {
            // lock the method to ensure a log buffer write isnt interrupted by another request (requests are queued)
            lock (_loggerLock)
            {
                if (!_logInit)
                {
                    // flush wont do sh!t until after the first 1024 bytes, so get that part out the way
                    Response.WriteAsync(_logSetup + new string(' ', 1024 - _logSetup.Length));
                    Response.Body.Flush();
                    _logInit = true;
                }

                Response.WriteAsync(@$"{description}<br />");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConfig()
        {
            try
            {
                // remove blank and irrelevant entries
                var config = Request.Form.Keys.Where(k =>
                    !new[] { "ipaddress", "password", "__RequestVerificationToken" }.Contains(k) &&
                    !string.IsNullOrEmpty(Request.Form[k])
                ).ToDictionary(k => k, k => Request.Form[k].ToString());

                var result = await Tasmota.SetDeviceConfig(Request.Form["ipaddress"], Request.Form["password"], config);

                TempData["info"] = $"Sent backlog to device for configuration. Response was:<br />{result.Item3}";

                ViewBag.IPAddress = result.Item1;
                ViewBag.Password = result.Item2;

                return View();
            }
            catch (Exception err)
            {
                TempData["danger"] = $"Failed. {err.Message}";
                return Redirect($"/tasmota/view?ipaddress={Request.Form["ipaddress"]}&password={Request.Form["password"]}");
            }
        }

        [ValidateAntiForgeryToken]
        public void UpdateConfigs()
        {
            try
            {
                // remove blank and irrelevant entries
                var config = Request.Form.Keys.Where(k =>
                    !new[] { "ipaddresses", "password", "__RequestVerificationToken" }.Contains(k) &&
                    !string.IsNullOrEmpty(Request.Form[k])
                ).ToDictionary(k => k, k => Request.Form[k].ToString());

                var ipaddresses = Request.Form["ipaddresses"].ToString().Split(',');
                var tasks = new List<Task>();

                // begin all config tasks in parallel
                foreach (var ipaddress in ipaddresses) {
                    tasks.Add(Tasmota.SetDeviceConfig(ipaddress, Request.Form["password"], config)
                        .ContinueWith(task => Log(task.IsFaulted ? $"{ipaddress} configuration failed - {task.Exception.Message}" : $"{task.Result.Item1} configuration sent. Response was: {task.Result.Item3}")));
                }

                // wait for all tasks to complete
                Task.WaitAll(tasks.ToArray());

                Log("<p><strong>If any devices were updated, they may need time to restart after the configuration update (20 seconds or so)</strong></p>");
            }
            catch (Exception err)
            {
                Log($"Failed - {err.Message}");
            }
        }
    }
}