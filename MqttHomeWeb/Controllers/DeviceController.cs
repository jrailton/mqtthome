using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Config;
using Newtonsoft.Json;

namespace MqttHomeWeb.Controllers
{
    public class DeviceController : Controller
    {
        [HttpGet]
        public IActionResult Add(string id, string friendlyName) {
            return View("AddEdit", new Device { Id = id, FriendlyName = friendlyName, SaveSensorValuesToDatabase = true });
        }

        [HttpPost]
        public IActionResult Add(Device device)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // read device config
                    var content = System.IO.File.ReadAllText("devices.json");
                    var deviceConfig = JsonConvert.DeserializeObject<DeviceConfig>(content);

                    var existing = deviceConfig.Devices.SingleOrDefault(d => d.Id == device.Id);

                    if (existing != null)
                        deviceConfig.Devices.Remove(existing);

                    deviceConfig.Devices.Add(device);

                    // write device config
                    System.IO.File.WriteAllText("devices.json", JsonConvert.SerializeObject(deviceConfig));

                    Program.MqttHomeController.LoadDevices();

                    TempData["success"] = "The device was added and the MqttHome devices were reloaded";

                    return Redirect("/");
                }
                catch (Exception err) {
                    TempData["danger"] = "The device update failed - " + err.Message;
                }
            }

            return View("AddEdit", device);
        }
    }
}