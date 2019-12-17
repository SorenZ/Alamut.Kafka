using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsGeneric : IGenericSubscriber<Foo>
    {
        public Task Handle(Foo message, CancellationToken token)
        {
            Console.WriteLine($"Received message { message.Bar }");

            return Task.CompletedTask;
        }
    }
}