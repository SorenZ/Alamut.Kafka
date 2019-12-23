namespace Alamut.Kafka.Models
{
    public class Message : IMessage
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public object Data { get; set; }
    }

    public class Message<T> : IMessage
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public T Data { get; set; }
    }

    public interface IMessage
    {
        string Id { get; set; }
        string EventName { get; set; }
    }
}