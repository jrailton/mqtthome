using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MqttHome.Config
{
    public class DeviceConfig
    {
        public List<Device> Devices { get; set; } = new List<Device>();

        public static List<SelectListItem> DeviceTypes(string selectedType) {
            var output = new List<SelectListItem>();

            var baseType = typeof(MqttHome.Device);
            var types = Assembly.GetAssembly(baseType).GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));

            return types.Select(t => new SelectListItem { Text = t.Name, Selected = t.Name == selectedType }).ToList();
        }
    }
}
