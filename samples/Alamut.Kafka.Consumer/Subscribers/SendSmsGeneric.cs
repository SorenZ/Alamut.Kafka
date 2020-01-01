using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    [Topics(Startup.TestTopic)]
    public class SendSmsGeneric : IMessageHandler<FooMessage>
    {
        public Task Handle(FooMessage message, CancellationToken token)
        {
            Console.WriteLine($"Received message <{ message.Bar }>");

            //await Task.Delay(TimeSpan.FromSeconds(10));

            //Console.WriteLine($"Processed message <{ message.Bar }>");

            return Task.CompletedTask;
        }
    }
}