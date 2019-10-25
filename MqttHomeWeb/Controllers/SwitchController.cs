using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MqttHomeWeb.Controllers
{
    public class SwitchController : Controller
    {
        public IActionResult Off(string id)
        {
            try
            {
                Program.MqttHomeController.MqttDevices.Single(d => d.Id == id).SwitchOff("UI Request");
                TempData["success"] = $"Switch {id} was sent a request to switch OFF";
            }
            catch (Exception err)
            {
                TempData["danger"] = err.Message;
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult On(string id)
        {
            try
            {
                Program.MqttHomeController.MqttDevices.Single(d => d.Id == id).SwitchOn("UI Request", null);
                TempData["success"] = $"Switch {id} was sent a request to switch ON";
            }
            catch (Exception err)
            {
                TempData["danger"] = err.Message;
            }
            return RedirectToAction("Index", "Home");
        }
    }
}