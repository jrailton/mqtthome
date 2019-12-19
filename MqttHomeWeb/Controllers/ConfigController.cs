using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Config;
using Newtonsoft.Json;

namespace MqttHomeWeb.Controllers
{
    public class ConfigController : Controller
    {
        public IActionResult Index(AppSettings settings)
        {
            if (Request.Method == "POST")
            {
                // do internal validation

                if (ModelState.IsValid)
                {
                    try
                    {
                        // save settings to json config
                        System.IO.File.WriteAllText("appsettings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));

                        // update live values
                        Program.MqttHomeController.Settings = settings;

                        // success message
                        TempData["success"] = "appsettings.json was updated successfully";

                        return RedirectToAction("Index"); // so that a reload wont remember the original form post
                    }
                    catch (Exception err)
                    {
                        TempData["danger"] = $"appsettings.json update failed. {err.Message}";
                    }

                }
            }
            else {
                settings = Program.MqttHomeController.Settings;
            }

            return View(settings);
        }
    }
}