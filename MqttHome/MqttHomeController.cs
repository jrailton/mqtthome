using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using InfluxDB.LineProtocol.Payload;
using log4net;
using Microsoft.Extensions.Configuration;
using MqttHome.Influx;
using MqttHome.Mqtt;
using MqttHome.Mqtt.BrokerCommunicator;
using MqttHome.Mqtt.Devices;
using MqttHome.Mqtt.Devices.Environment;
using MqttHome.Mqtt.Devices.Sonoff;
using MqttHome.Mqtt.Devices.Victron;
using MqttHome.WebSockets;
using Newtonsoft.Json;

namespace MqttHome
{
    public class MqttHomeController
    {
        public DateTime StartupTime;

        public WebsocketManager WebsocketManager;

        public List<MqttCommunicator> MqttCommunicators = new List<MqttCommunicator>();
        public IQueryable<MqttDevice> MqttDevices;

        public InfluxCommunicator InfluxCommunicator;
        public List<string> MqttDeviceTopics;
        public RuleEngine RuleEngine;

        public bool Debug;

        // allows sensor values to NOT get saved e.g. time sensor
        public bool SaveAllSensorValuesToDatabaseEveryTime = true;
        
        public double Longitude;
        public double Latitude;

        public MqttHomeLogger RuleLog;
        public MqttHomeLogger DeviceLog;
        public MqttHomeLogger GeneralLog;
        public MqttHomeLogger InfluxLog;
        public MqttHomeLogger MqttLog;

        public MqttHomeController(IConfiguration config, ILog ruleLog, ILog deviceLog, ILog generalLog, ILog influxLog, ILog mqttLog, List<MqttBroker> mqttBrokers, string influxUrl = "http://localhost:8086", string influxDatabase = "home_db", WebsocketManager wsm = null)
        {
            try
            {
                StartupTime = DateTime.Now;

                SaveAllSensorValuesToDatabaseEveryTime = config["SaveAllSensorValuesToDatabaseEveryTime"]?.Contains("true") ?? true;

                double.TryParse(config["Latitude"], out Latitude);
                double.TryParse(config["Longitude"], out Longitude);

                WebsocketManager = wsm;

                MqttLog = new MqttHomeLogger(wsm, mqttLog);
                RuleLog = new MqttHomeLogger(wsm, ruleLog);
                DeviceLog = new MqttHomeLogger(wsm, deviceLog);
                GeneralLog = new MqttHomeLogger(wsm, generalLog);
                InfluxLog = new MqttHomeLogger(wsm, influxLog);

                Debug = false;

                InfluxCommunicator = new InfluxCommunicator(InfluxLog, new Uri(influxUrl), influxDatabase);

                LoadDevices();

                foreach (IStatefulDevice device in MqttDevices.Where(d => d is IStatefulDevice))
                {
                    device.StateChanged += Device_StateChanged;
                }

                foreach (ISensorDevice<ISensorData> device in MqttDevices.Where(d => d is ISensorDevice<ISensorData>))
                {
                    device.SensorDataChanged += Device_SensorDataChanged;
                }

                DeviceLog.Debug($"Added {MqttDevices.Count()} MQTT devices...");

                // this is a hack which needs more thought
                MqttDeviceTopics = MqttDevices.SelectMany(d => d.AllTopics).Distinct().ToList();

                foreach (var broker in mqttBrokers)
                    MqttCommunicators.Add(new MqttCommunicator(this, broker));

                RuleEngine = new RuleEngine(this);

                DeviceLog.Debug($@"Started listening to {MqttDevices.Count()} MQTT devices. Topic list is:
{string.Join(Environment.NewLine, MqttDeviceTopics)}");

            }
            catch (Exception err)
            {
                GeneralLog.Error($"Exception in MqttHomeController.ctor - {err.Message}", err);
            }
        }

        private void LoadDevices()
        {

            DeviceConfig deviceConfig;
            var devices = new List<MqttDevice>();

            // add time device
            devices.Add(new TimeDevice(this, "time", "Time Sensor (System)"));

            try
            {
                var content = File.ReadAllText("devices.json");
                deviceConfig = JsonConvert.DeserializeObject<DeviceConfig>(content);
                var allMqttDeviceTypes = Assembly.GetExecutingAssembly().GetTypes();

                foreach (var device in deviceConfig.Devices)
                {
                    var type = allMqttDeviceTypes.First(t => t.Name == device.Type);
                    devices.Add((MqttDevice)Activator.CreateInstance(type, this, device.Id, device.FriendlyName, device.Parameters));
                }

                DeviceLog.Info($"LoadDevices :: Loaded {deviceConfig.Devices.Count} devices from devices.json");

            }
            catch (Exception err)
            {
                DeviceLog.Error($"LoadDevices :: Failed to load devices. {err.Message}", err);
            }

            MqttDevices = devices.AsQueryable();
        }

        private void Device_StateChanged(object sender, StateChangedEventArgs e)
        {
            var device = (MqttDevice)sender;

            var lpp = new LineProtocolPoint("Switch",
                new Dictionary<string, object>{
                    { "On", (e.PowerOn ? "1" : "0") }
                },
                new Dictionary<string, string>
                {
                    {"device", device.Id}
                });

            InfluxCommunicator.Write(lpp);

            RuleEngine.OnDeviceStateChanged(device, e);
        }

        private void Device_SensorDataChanged(object sender, SensorDataChangedEventArgs e)
        {
            var device = (MqttDevice)sender;
            var sensorDevice = device as ISensorDevice<ISensorData>;

            // make sure some values were specified before saving to db or running rules
            if ((e.ChangedValues?.Count() ?? 0) > 0)
            {
                if (sensorDevice.SaveSensorValuesToDatabase)
                {
                    var lpp = new LineProtocolPoint(device.DeviceClass.ToString(),
                    e.ChangedValues,
                    new Dictionary<string, string>
                    {
                    {"device", device.Id}
                    });

                    InfluxCommunicator.Write(lpp);
                }

                RuleEngine.OnDeviceSensorDataChanged(sensorDevice, e.ChangedValues);
            }
        }

        public void Start()
        {
            // start all mqttbroker connections
            foreach (var communicator in MqttCommunicators)
                communicator.Start();
        }
    }
}
