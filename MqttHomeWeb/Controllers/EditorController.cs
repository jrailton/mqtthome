﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MqttHomeWeb.Controllers
{
    public class EditorController : Controller
    {
        [HttpGet]
        public IActionResult Index(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ViewBag.Filename = id;
                ViewBag.FileContent = System.IO.File.ReadAllText(System.IO.Path.Combine(Program.RootFolderPath, id));
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index(string id, string newcontent)
        {
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(Program.RootFolderPath, id), newcontent);
                TempData["success"] = $"Changes to the file {id} were saved. You will need to restart the controller to update the changes.";
            }
            catch (Exception err) {
                TempData["danger"] = $"Changes to the file {id} were not saved - {err.Message}";
            }

            return RedirectToAction("Index", new { id = id });
        }
    }
}