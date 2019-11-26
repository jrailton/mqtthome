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
using MqttHome.Mqtt.BrokerCommunicator;
using MqttHome.WebSockets;
using MqttHomeWeb.Models;
using Newtonsoft.Json;

namespace MqttHomeWeb
{
    public class Program
    {
        public static MqttHomeController MqttHomeController;
        public static WebsocketManager WebsocketManager;
        public static DateTime StartupTime;
        public static string RootFolderPath;
        public static ILog GeneralLog;

        public static IConfiguration Config;

        public static void Main(string[] args)
        {
            StartupTime = DateTime.Now;

            // configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // setup general logger
            GeneralLog = LogManager.GetLogger(logRepository.Name, "GeneralLog");

            var webHost = CreateWebHostBuilder(args).Build();

            WebsocketManager = (WebsocketManager)webHost.Services.GetService(typeof(WebsocketManager));

            Config = Helpers.ConfigurationManager.AppSetting;

            RestartMqttHomeController();

            webHost.Run();
        }

        public static void RestartMqttHomeController()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()).Name;

            var mqttBrokers = Program.Config.GetSection("MqttBrokers").Get<List<MqttBroker>>();

            MqttHomeController = new MqttHomeController(Program.Config,
                LogManager.GetLogger(logRepository, "RuleLog"),
                LogManager.GetLogger(logRepository, "DeviceLog"),
                GeneralLog,
                LogManager.GetLogger(logRepository, "InfluxLog"),
                LogManager.GetLogger(logRepository, "MqttLog"),
                mqttBrokers,
                WebsocketManager
            );

            MqttHomeController.Start();
        }

        public static void StopMqttHomeController() {
            MqttHomeController = null;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
