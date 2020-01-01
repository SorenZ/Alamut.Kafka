using Alamut.Abstractions.Messaging;

namespace Alamut.Kafka.Models
{
    public class Foo
    {
        public string Bar { get; set; }
    }

    public class WrongModel
    {
        public string UnknownName { get; set; }
    }

    public class FooMessage : IMessage
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public string Bar { get; set; }
        public bool AcknowledgeRequested { get; set;  }
        public string AcknowledgeTopic { get; set;  }
    }
}