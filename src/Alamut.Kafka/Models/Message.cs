namespace Alamut.Kafka.Models
{
    public class Message
    {
        public Message(object data)
        {
            this.Data = data;
        }
        public Message(object data, string eventName)
        {
            Data = data;
            EventName = eventName;
        }

        public string EventName { get; set; }
        public object Data { get; }
    }
}