using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsGeneric : ISubscriber<Foo>
    {
        public Task Handle(Foo message, CancellationToken token)
        {
            Console.WriteLine($"Received message { message.Bar }");

            if(message.Bar == "item2")
            { throw new Exception("test item 2 exception");}

            return Task.CompletedTask;
        }
    }
}