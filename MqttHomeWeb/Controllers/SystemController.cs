using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MqttHome;
using MqttHomeWeb.Models;

namespace MqttHomeWeb.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        public void HideSystemMessage(string id)
        {
            var cunt = Program.MqttHomeController.SystemMessages.SingleOrDefault(m => m.Id == id);
            Program.MqttHomeController.SystemMessages.Remove(cunt);
        }

        public void HideAllSystemMessages(SystemMessageType id)
        {
            Program.MqttHomeController.SystemMessages.RemoveAll(m => m.Type == id);
        }

        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View("Switches", Program.MqttHomeController);
        }

        public IActionResult Rules()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult People()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult Details()
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

        [Authorize(Roles = "Admin")]
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

        public IActionResult DetailsContent()
        {
            return PartialView("_DetailsContent", Program.MqttHomeController);
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

        [Authorize(Roles = "Admin")]
        public IActionResult Restart()
        {
            Program.RestartMqttHomeController();

            Program.GeneralLog.Warn($"{Request.HttpContext.User.Identity.Name} restarted the MqttHomeController");

            TempData["success"] = "MqttHomeController was restarted";

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        public string Kill()
        {
            Program.StopMqttHomeController();

            Program.GeneralLog.Warn($"{Request.HttpContext.User.Identity.Name} killed the MqttHomeController");

            TempData["success"] = "MqttHomeController was killed";

            return $"{Request.HttpContext.User.Identity.Name} killed the MqttHomeController. You will need to manually restart the service from the Web Server";
        }
    }
}
