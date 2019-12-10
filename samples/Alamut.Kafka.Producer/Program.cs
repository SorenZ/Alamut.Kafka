using System;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("start Kafka producer");

            IPublisher publisher = new KafkaProducer(new Models.KafkaConfig { BootstrapServers = "10.104.51.12:9092,10.104.51.13:9092,10.104.51.14:9092" });
            
            while (true)
            {
                var message = Console.ReadLine();
                await publisher.Publish("mobin-soft", new Message(message));
            }
        }
    }
}
