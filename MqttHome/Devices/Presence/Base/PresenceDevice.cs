using System;
using System.Collections.Generic;
using System.Text;
using MqttHome.Mqtt;

namespace MqttHome.Presence
{
    public abstract class PresenceDevice : Device, IPresenceDevice
    {
        public event EventHandler<PresenceChangedEventArgs> PresenceChanged;

        public bool SaveSensorValuesToDatabase { get; }

        public PresenceDevice(MqttHomeController controller, Config.Device config) : base(controller, config)
        {
            DeviceClass = DeviceClass.Person;
        }

        public void Woft(object sender, Person person) {
            PresenceChanged?.Invoke(sender, new PresenceChangedEventArgs { Person = person });
        }

        public List<Person> People { get; set; } = new List<Person>();
    }

    public interface IPresenceDevice : IDevice
    {
        event EventHandler<PresenceChangedEventArgs> PresenceChanged;

        bool SaveSensorValuesToDatabase { get; }

        public List<Person> People { get; }
    }

    public class PresenceChangedEventArgs : EventArgs
    {
        public Person Person;
    }
}
