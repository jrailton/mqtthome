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
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.MqttHomeController);
        }

        public IActionResult IndexContent() {
            return PartialView("_IndexContent", Program.MqttHomeController);
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
