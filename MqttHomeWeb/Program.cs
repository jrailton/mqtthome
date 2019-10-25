using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MqttHome;

namespace MqttHomeWeb
{
    public class Program
    {
        public static MqttHomeController MqttHomeController;

        public static void Main(string[] args)
        {
            // configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            RestartMqttHomeController();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static void RestartMqttHomeController()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()).Name;

            MqttHomeController = new MqttHomeController(false,
                LogManager.GetLogger(logRepository, "RuleLog"),
                LogManager.GetLogger(logRepository, "DeviceLog"),
                LogManager.GetLogger(logRepository, "GeneralLog"),
                LogManager.GetLogger(logRepository, "InfluxLog"),
                LogManager.GetLogger(logRepository, "MqttLog"),
                MqttHomeWeb.Helpers.ConfigurationManager.AppSetting["MqttBrokerIp"],
                int.Parse(MqttHomeWeb.Helpers.ConfigurationManager.AppSetting["MqttBrokerPort"]),
                MqttHomeWeb.Helpers.ConfigurationManager.AppSetting["InfluxDbUrl"],
                MqttHomeWeb.Helpers.ConfigurationManager.AppSetting["InfluxDbDatabase"]
            );

            MqttHomeController.Start();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
