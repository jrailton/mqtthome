using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using MQTTnet;

namespace InfluxDbLoader.Mqtt
{
    public abstract class MqttDevice
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public MqttDevice(string id)
        {
            Id = id;

            StateTopic = StateTopic?.Replace("@id", id);
            SensorTopic = SensorTopic.Replace("@id", id);
            RebootDevice = new MqttCommand(id, $"cmnd/{id}/Restart", "1");
        }

        public string Id;
        public virtual string StateTopic
        {
            get => $"tele/{Id}/STATE";
            set { }
        }

        // commands
        public virtual MqttCommand SetPowerStateOn { get; set; }
        public virtual MqttCommand SetPowerStateOff { get; set; }
        public virtual MqttCommand RebootDevice { get; set; }

        public virtual string SensorTopic
        {
            get => $"tele/{Id}/SENSOR";
            set { }
        }
        public abstract MqttDeviceType DeviceType { get; set; }
        public abstract MqttDeviceClass DeviceClass { get; set; }

        private bool _powerOn;
        public bool PowerOn
        {
            get => _powerOn;
            protected set {
                _powerOn = value;
                WriteToInflux(_powerOn);
            }
        }

        private SensorData _sensorData;
        public SensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                WriteToInflux(_sensorData);
            }
        }

        public bool IsSubscribedToStateTopic(string topic)
        {
            return (StateTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsSubscribedToSensorTopic(string topic)
        {
            return (SensorTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public abstract void ParseStatePayload(MqttApplicationMessage message);
        public abstract void ParseSensorPayload(MqttApplicationMessage message);

        private void WriteToInflux(SensorData data)
        {
            var lpp = new LineProtocolPoint(DeviceClass.ToString(),
                data.ToDictionary(),
                new Dictionary<string, string>
                {
                    {"device", Id}
                });

            Program.InfluxCommunicator.Write(lpp);
        }

        private void WriteToInflux(bool powerOn)
        {
            var lpp = new LineProtocolPoint("Switch",
                new Dictionary<string, object>{
                    { "On", (powerOn ? "1" : "0") }
                },
                new Dictionary<string, string>
                {
                    {"device", Id}
                });

            Program.InfluxCommunicator.Write(lpp);
        }

        public void SwitchOn()
        {
            SetPowerStateOn.Execute();
        }

        public void SwitchOff()
        {
            SetPowerStateOff.Execute();
        }
    }

    public class MqttCommand
    {
        public MqttCommand(string deviceId, string topic, string payload) : this(deviceId, topic, Encoding.UTF8.GetBytes(payload))
        {
        }

        public MqttCommand(string deviceId, string topic, IEnumerable<byte> payload)
        {
            Topic = topic;
            Payload = payload;
            DeviceId = deviceId;
        }

        public string DeviceId { get; set; }
        public string Topic { get; set; }
        public IEnumerable<byte> Payload { get; set; }

        public void Execute()
        {
            Program.MqttCommunicator.PublishCommand(this);
        }
    }
}
