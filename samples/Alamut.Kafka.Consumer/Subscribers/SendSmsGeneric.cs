using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging;
using Alamut.Abstractions.Structure;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    [Topics(Startup.TestTopic)]
    public class SendSmsGeneric : IMessageHandler<FooMessage>
    {
        private readonly IPublisher _publisher;
        public SendSmsGeneric(IPublisher publisher)
        {
            _publisher = publisher;

        }

        public Task Handle(FooMessage message, CancellationToken token)
        {
            Console.WriteLine($"Received message <{ message.Bar }>");


            if (message.AcknowledgeRequested)
            {
                _publisher.Publish(message.AcknowledgeTopic,
                    new AcknowledgeMessage
                    {
                        Id = message.Id,
                        Result = Result.Okay()
                    });
            }

            //await Task.Delay(TimeSpan.FromSeconds(10));

            //Console.WriteLine($"Processed message <{ message.Bar }>");

            return Task.CompletedTask;
        }
    }
}