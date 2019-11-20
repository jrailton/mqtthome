namespace MqttHome
{
    public abstract class Device : IDevice
    {
        public Device(MqttHomeController controller, string id, string friendlyName) {
            Controller = controller;
            Id = id;
            FriendlyName = friendlyName;
        }

        public MqttHomeController Controller { get; protected set; }

        public string FriendlyName { get; protected set; }

        public string Id { get; set; }

        public virtual DeviceType DeviceType { get; set; }

        public virtual DeviceClass DeviceClass { get; set; }

    }
}
