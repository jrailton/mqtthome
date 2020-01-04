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
using MqttHome.Config;
using MqttHome.Devices.Base;
using MqttHome.Devices.Serial.Base;
using MqttHome.Influx;
using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;
using MqttHome.Mqtt.Devices.Environment;
using MqttHome.Mqtt.Devices.Sonoff;
using MqttHome.Mqtt.Devices.Victron;
using MqttHome.Presence;
using MqttHome.Presence.Huawei;
using MqttHome.WebSockets;
using Newtonsoft.Json;

namespace MqttHome
{
    public class MqttHomeController
    {
        public DateTime StartupTime;

        public WebsocketManager WebsocketManager;

        public List<MqttCommunicator> MqttCommunicators = new List<MqttCommunicator>();
        public List<MqttDevice> MqttDevices;
        public List<PresenceDevice> PresenceDevices;
        public List<SerialDevice> SerialDevices;

        public SystemMessages SystemMessages = new SystemMessages();

        public InfluxCommunicator InfluxCommunicator = null;
        public List<string> MqttDeviceTopics;
        public RuleEngine RuleEngine;
        public List<Person> People = new List<Person>();

        public MqttHomeLogger RuleLog;
        public MqttHomeLogger DeviceLog;
        public MqttHomeLogger GeneralLog;
        public MqttHomeLogger InfluxLog;
        public MqttHomeLogger MqttLog;

        public AppSettings Settings;

        public MqttHomeController(WebsocketManager wsm = null)
        {
            try
            {
                // set jsonconvert global settings
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                // setup logging
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()).Name;

                MqttLog = new MqttHomeLogger(this, LogManager.GetLogger(logRepository, "MqttLog"));
                RuleLog = new MqttHomeLogger(this, LogManager.GetLogger(logRepository, "RuleLog"));
                DeviceLog = new MqttHomeLogger(this, LogManager.GetLogger(logRepository, "DeviceLog"));
                GeneralLog = new MqttHomeLogger(this, LogManager.GetLogger(logRepository, "GeneralLog"));
                InfluxLog = new MqttHomeLogger(this, LogManager.GetLogger(logRepository, "InfluxLog"));

                StartupTime = DateTime.Now;

                // load app settings
                LoadAppSettings();

                // keep reference to websocketmanager
                WebsocketManager = wsm;

                if (Settings.InfluxDbEnabled && !string.IsNullOrEmpty(Settings.InfluxDbUrl) && !string.IsNullOrEmpty(Settings.InfluxDbDatabase))
                    InfluxCommunicator = new InfluxCommunicator(InfluxLog, Settings.InfluxDbUrl, Settings.InfluxDbDatabase);

                LoadPeople();

                LoadDevices();

                SetupDeviceEventListeners();

                // this is a hack which needs more thought
                MqttDeviceTopics = MqttDevices.SelectMany(d => d.AllTopics).Distinct().ToList();

                if (Settings.MqttBrokers != null)
                    foreach (var broker in Settings.MqttBrokers)
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

        private void LoadAppSettings() {
            try
            {
                // read config
                var content = File.ReadAllText("appsettings.json");
                Settings = JsonConvert.DeserializeObject<AppSettings>(content);
            }
            catch (Exception err)
            {
                GeneralLog.Error($"Failed to load appsettings.json - {err.Message}", err);
                Settings = new AppSettings();
            }
        }

        private void LoadPeople()
        {
            try
            {
                // read device config
                var content = File.ReadAllText("people.json");
                People = JsonConvert.DeserializeObject<List<Person>>(content);

                DeviceLog.Info($"LoadPeople :: Loaded {People.Count()} people from people.json");

            }
            catch (Exception err)
            {
                DeviceLog.Error($"LoadPeople :: Failed to load people. {err.Message}", err);
            }

        }

        public void LoadDevices()
        {
            DeviceConfig deviceConfig;

            MqttDevices = new List<MqttDevice>();
            SerialDevices = new List<SerialDevice>();
            PresenceDevices = new List<PresenceDevice>();

            // add time device
            MqttDevices.Add(new TimeDevice(this));

            try
            {
                // read device config
                var content = File.ReadAllText("devices.json");
                deviceConfig = JsonConvert.DeserializeObject<DeviceConfig>(content);
                var allDeviceTypes = Assembly.GetExecutingAssembly().GetTypes();

                foreach (var device in deviceConfig.Devices)
                {
                    var type = allDeviceTypes.First(t => t.Name == device.Type);

                    var interfaces = type.GetInterfaces();

                    // add the device to the relevant list
                    // TODO: Change the master list to be an IDevice list
                    if (interfaces.Any(i => i.Name == "IMqttDevice"))
                    {
                        MqttDevices.Add((MqttDevice)Activator.CreateInstance(type, this, device));
                    }
                    else if (interfaces.Any(i => i.Name == "IPresenceDevice"))
                    {
                        PresenceDevices.Add((PresenceDevice)Activator.CreateInstance(type, this, device));
                    }
                    else if (interfaces.Any(i => i.Name == "ISerialDevice"))
                    {
                        SerialDevices.Add((SerialDevice)Activator.CreateInstance(type, this, device));
                    }
                }

                DeviceLog.Info($"LoadDevices :: Loaded {deviceConfig.Devices.Count} devices from devices.json ({MqttDevices.Count} MQTT, {SerialDevices.Count} Serial, {PresenceDevices.Count} Presence)");
            }
            catch (Exception err)
            {
                DeviceLog.Error($"LoadDevices :: Failed to load devices. {err.Message}", err);
            }
        }

        private void SetupDeviceEventListeners()
        {
            foreach (ISwitchDevice device in MqttDevices.Where(d => d is ISwitchDevice))
            {
                device.StateChanged += Device_StateChanged;
            }

            foreach (IMqttSensorDevice<ISensorData> device in MqttDevices.Where(d => d is IMqttSensorDevice<ISensorData>))
            {
                device.SensorDataChanged += Device_SensorDataChanged;
            }

            foreach (ISensorDevice<ISensorData> device in SerialDevices.Where(d => d is ISensorDevice<ISensorData>))
            {
                device.SensorDataChanged += Device_SensorDataChanged;
            }

            foreach (IPresenceDevice device in PresenceDevices.Where(d => d is IPresenceDevice))
            {
                device.PresenceChanged += Device_PresenceChanged;
            }
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

            InfluxCommunicator?.Write(lpp);

            RuleEngine?.OnDeviceStateChanged(device, e);
        }

        private void Device_SensorDataChanged(object sender, SensorDataChangedEventArgs e)
        {
            try
            {
                var device = (Device)sender;
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

                        InfluxCommunicator?.Write(lpp);
                    }

                    RuleEngine?.OnDeviceSensorDataChanged(sensorDevice, e.ChangedValues);
                }
            }
            catch (Exception err) {
                DeviceLog.Error($"MqttHomeController.Device_SensorDataChanged :: Error - {err.Message}", err);
            }
        }

        private void Device_PresenceChanged(object sender, PresenceChangedEventArgs e)
        {
            var device = (PresenceDevice)sender;
            var presenceDevice = device as IPresenceDevice;

            // maintain live list of People
            var person = People.SingleOrDefault(p => p.Id == e.Person.Id);

            if (person != null && person.Present != e.Person.Present)
            {
                person.Present = e.Person.Present;
                person.PresenceChanged = e.Person.PresenceChanged;

                if (presenceDevice.SaveSensorValuesToDatabase)
                {
                    var lpp = new LineProtocolPoint("Person",
                        new Dictionary<string, object>{
                    { "Present", (e.Person.Present ? "1" : "0") }
                        },
                        new Dictionary<string, string>
                        {
                    {"device", device.Id}
                        });

                    InfluxCommunicator?.Write(lpp);
                }
            }

            RuleEngine?.OnPresenceChanged(person);
        }

        public void Start()
        {
            // start all mqttbroker connections
            foreach (var communicator in MqttCommunicators)
                communicator.Start();
        }
    }
}
