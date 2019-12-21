using System;
using System.Threading.Tasks;
using Alamut.Helpers.DomainDriven;
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
                // var dynamicMessage = new Foo
                // {
                //     Bar = message
                // };
                var dynamicMessage = new FooMessage
                {
                    Id = IdGenerator.GetNewId(),
                    EventName = "Alamut.Foo.Create",
                    Bar = message
                };
                await publisher.Publish("mobin-soft", dynamicMessage);
                // await publisher.Publish("mobin-soft", "send sms to 0912");
            }
        }
    }
}
