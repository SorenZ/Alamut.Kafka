namespace Alamut.Kafka.Models
{
    public class Message
    {
        public Message(object data)
        {
            this.Data = data;
        }
        
        public string EventName { get; set; }
        public object Data { get; }
    }
}