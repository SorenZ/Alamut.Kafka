using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsGeneric : ISubscriber<FooMessage>
    {
        public async Task Handle(FooMessage message, CancellationToken token)
        {
            Console.WriteLine($"Received message <{ message.Bar }>");

            // await Task.Delay(TimeSpan.FromSeconds(20));

            // Console.WriteLine($"Processed message <{ message.Bar }>");
        }
    }
}