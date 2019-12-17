using System;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

using Confluent.Kafka;
using Newtonsoft.Json;

namespace Alamut.Kafka
{
    public class KafkaProducer : IPublisher
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(KafkaConfig kafkaConfig)
        {
            var config = new ProducerConfig { BootstrapServers = kafkaConfig.BootstrapServers 
                ?? throw new ArgumentNullException(nameof(kafkaConfig.BootstrapServers)) };
            
             _producer = new ProducerBuilder<Null,string>(config).Build();
        }

        public async Task Publish(string topic, string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message));}
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

             try
            {
                // var serializedMessage = JsonConvert.SerializeObject(message);

                // Note: Awaiting the asynchronous produce request below prevents flow of execution
                // from proceeding until the acknowledgement from the broker is received (at the 
                // expense of low throughput).
                var deliveryReport = await _producer.ProduceAsync(
                    topic,
                    new Message<Null, string> {Value = message});

                Console.WriteLine($"delivered to: {deliveryReport.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                throw;
            }


        }

        public async Task Publish(string topic, object message)
        {
            await Publish(topic, JsonConvert.SerializeObject(message));
        }
    }
}