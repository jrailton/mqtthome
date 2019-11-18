using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MqttHomeWeb.Controllers
{
    public class EditorController : Controller
    {
        public static string GetAutocompleteList() {
            var items = new List<string>();

            items.AddRange(Program.MqttHomeController.RuleEngine.ConditionConfig.Conditions.Select(c => c.Id));
            items.AddRange(Program.MqttHomeController.RuleEngine.RuleConfig.Rules.Select(c => c.Name));
            items.AddRange(Program.MqttHomeController.MqttDevices.Select(c => c.Id));

            return string.Join(",", items.Select(i => $"'{i}'"));
        }

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

        public IActionResult Delete(string id) {

            try {
                System.IO.File.Delete(System.IO.Path.Combine(Program.RootFolderPath, id));
                TempData["success"] = $"{id} was deleted";
            }
            catch (Exception err) {
                TempData["danger"] = $"Failed to delete {id} - {err.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}