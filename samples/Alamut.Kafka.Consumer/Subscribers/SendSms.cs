using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSms : ISubscriber
    {
        public Task Handle(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message {message.Data}");

            return Task.CompletedTask;
        }
    }
}