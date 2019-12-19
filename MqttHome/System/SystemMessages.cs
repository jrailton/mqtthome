using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome
{
    public class SystemMessages : List<Message>
    {

        public SystemMessages() : base() { }

        public void Add(SystemMessageType type, string description)
        {
            Add(new Message
            {
                Type = type,
                Description = description,
                Generated = DateTime.Now,
                Id = Guid.NewGuid().ToString()
            });
        }
    }

    public class Message
    {
        public SystemMessageType Type;
        public DateTime Generated;
        public string Id;
        public string Description;
    }

    public enum SystemMessageType
    {
        info = 1,
        warning = 2,
        danger = 3
    }
}
