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
using MqttHome.Influx;
using MqttHome.Mqtt;
using MqttHome.Mqtt.BrokerCommunicator;
using MqttHome.Mqtt.Devices;
using MqttHome.Mqtt.Devices.Sonoff;
using MqttHome.Mqtt.Devices.Victron;
using MqttHome.WebSockets;
using Newtonsoft.Json;

namespace MqttHome
{
    public class MqttHomeController
    {
        public WebsocketManager WebsocketManager;

        public List<MqttCommunicator> MqttCommunicators = new List<MqttCommunicator>();
        public IQueryable<MqttDevice> MqttDevices;

        public InfluxCommunicator InfluxCommunicator;
        public List<string> MqttDeviceTopics;
        public RuleEngine RuleEngine;
        public bool Debug;

        public MqttHomeLogger RuleLog;
        public MqttHomeLogger DeviceLog;
        public MqttHomeLogger GeneralLog;
        public MqttHomeLogger InfluxLog;
        public MqttHomeLogger MqttLog;

        public MqttHomeController(bool debug, ILog ruleLog, ILog deviceLog, ILog generalLog, ILog influxLog, ILog mqttLog, List<MqttBroker> mqttBrokers, string influxUrl = "http://localhost:8086", string influxDatabase = "home_db", WebsocketManager wsm = null)
        {
            try
            {
                WebsocketManager = wsm;

                MqttLog = new MqttHomeLogger(wsm, mqttLog);
                RuleLog = new MqttHomeLogger(wsm, ruleLog);
                DeviceLog = new MqttHomeLogger(wsm, deviceLog);
                GeneralLog = new MqttHomeLogger(wsm, generalLog);
                InfluxLog = new MqttHomeLogger(wsm, influxLog);

                Debug = debug;

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

            try
            {
                var content = File.ReadAllText("devices.json");
                deviceConfig = JsonConvert.DeserializeObject<DeviceConfig>(content);
                var allMqttDeviceTypes = Assembly.GetExecutingAssembly().GetTypes();

                foreach (var device in deviceConfig.Devices)
                {
                    var type = allMqttDeviceTypes.First(t => t.Name == device.Type);
                    devices.Add((MqttDevice)Activator.CreateInstance(type, this, device.Id, device.FriendlyName));
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

            var lpp = new LineProtocolPoint(device.DeviceClass.ToString(),
                e.ChangedValues,
                new Dictionary<string, string>
                {
                    {"device", device.Id}
                });

            InfluxCommunicator.Write(lpp);

            RuleEngine.OnDeviceSensorDataChanged(device as ISensorDevice<ISensorData>, e.ChangedValues);
        }

        public void Start()
        {
            // start all mqttbroker connections
            foreach (var communicator in MqttCommunicators)
                communicator.Start();
        }
    }
}
