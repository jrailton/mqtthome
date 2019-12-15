using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Devices.Serial.Pylontech;

namespace MqttHomeWeb.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            var pt = new PylonCom("COM5", 115200);

            var mfi = pt.GetReply<PPManufacturerInfo>(new PPGetManufacturerInfo());

            return View(mfi);
        }
    }
}