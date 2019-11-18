using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MqttHome;
using MqttHomeWeb.Models;

namespace MqttHomeWeb.Controllers
{
    public class SystemController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Rules()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Conditions()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Devices()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Switches()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Sensors()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Logs(string id)
        {
            if (!string.IsNullOrEmpty(id)) {
                ViewBag.Filename = id;
                ViewBag.FileContent = string.Join(Environment.NewLine, System.IO.File.ReadLines(System.IO.Path.Combine(Program.RootFolderPath, id)).TakeLast(500));
            }

            return View();
        }

        public IActionResult DeleteLog(string id)
        {
            try
            {
                System.IO.File.Delete(System.IO.Path.Combine(Program.RootFolderPath, id));
                TempData["success"] = $"{id} was deleted";
            }
            catch (Exception err)
            {
                TempData["danger"] = $"Failed to delete {id} - {err.Message}";
            }

            return RedirectToAction("Logs");
        }

        public IActionResult IndexContent()
        {
            return PartialView("_IndexContent", Program.MqttHomeController);
        }

        public IActionResult SwitchContent()
        {
            return PartialView("_Switches", Program.MqttHomeController);
        }

        public IActionResult SensorContent()
        {
            return PartialView("_Sensors", Program.MqttHomeController);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Restart()
        {
            Program.RestartMqttHomeController();

            TempData["success"] = "MqttHomeController was restarted";

            return RedirectToAction("Index");
        }
    }
}
