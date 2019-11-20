using System;
using System.Collections.Generic;
using System.Text;
using MqttHome.Mqtt;

namespace MqttHome.Presence
{
    public abstract class PresenceDevice : Device, IPresenceDevice
    {
        public event EventHandler<PresenceChangedEventArgs> PresenceChangedEvent;

        public PresenceDevice(MqttHomeController controller, string id, string friendlyName, params string[] config) : base(controller, id, friendlyName)
        {
            DeviceClass = DeviceClass.Person;
        }

        public DateTime? PresenceChanged { get; private set; }

        private bool? _present;
        public bool? Present
        {
            get => _present;
            protected set
            {
                if (_present != value)
                {
                    _present = value;
                    PresenceChanged = DateTime.Now;

                    PresenceChangedEvent?.Invoke(this, new PresenceChangedEventArgs
                    {
                        Present = value.Value,
                    });
                }
            }
        }

        public abstract List<Person> People { get; }
    }

    public interface IPresenceDevice : IDevice
    {
        public List<Person> People { get; }
    }

    public class PresenceChangedEventArgs : EventArgs
    {
        public bool Present;
    }
}
