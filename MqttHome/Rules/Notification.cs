namespace MqttHome
{
    public class Notification
    {
        public Notification(string @class, string message)
        {
            Class = @class;
            Message = message;
        }
        public string Class { get; set; }
        public string Message { get; set; }
    }
}
