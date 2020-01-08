using System;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging;
using Alamut.Helpers.DomainDriven;
using Alamut.Kafka.Models;
using Alamut.Abstractions.Messaging.MessageContracts;

using Confluent.Kafka;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Alamut.Kafka
{
    /// <summary>
    /// implements Kafka producer as a publisher
    /// </summary>
    public class KafkaProducer : IPublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(KafkaConfig kafkaConfig, ILogger<KafkaProducer> logger)
        {
            _logger = logger;
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaConfig.BootstrapServers
                ?? throw new ArgumentNullException(nameof(kafkaConfig.BootstrapServers))
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task Publish(string topic, string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            try
            {
                // var serializedMessage = JsonConvert.SerializeObject(message);

                // Note: Awaiting the asynchronous produce request below prevents flow of execution
                // from proceeding until the acknowledgement from the broker is received (at the 
                // expense of low throughput).
                var deliveryReport = await _producer.ProduceAsync(
                    topic,
                    new Message<Null, string> { Value = message });

                _logger.LogTrace($"delivered to: {deliveryReport.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                throw;
            }


        }

        public async Task Publish(string topic, object message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            await Publish(topic, JsonConvert.SerializeObject(message));
        }

        public async Task Publish(string topic, IMessage message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            if(message.Id == null)
            { message.Id = IdGenerator.GetNewId(); }

            await Publish(topic, JsonConvert.SerializeObject(message));
        }
    }
}