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

        public KafkaProducer(ProducerConfig config, ILogger<KafkaProducer> logger)
        {
            _logger = logger;
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void Publish(string topic, string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            try
            {
                // Note: Awaiting the asynchronous produce request below prevents flow of execution
                // from proceeding until the acknowledgement from the broker is received (at the 
                // expense of low throughput).
                _producer.Produce(
                    topic,
                    new Message<Null, string> { Value = message });
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                throw;
            }
        }

        public void Publish(string topic, object message)
        {
            if (message == null) 
            { throw new ArgumentNullException(nameof(message)); }

            Publish(topic, JsonConvert.SerializeObject(message));
        }

        public void Publish(string topic, IMessage message)
        {
            if (message == null) 
            { throw new ArgumentNullException(nameof(message)); }

            if(message.Id == null)
            { message.Id = IdGenerator.GetNewId(); }

            Publish(topic, JsonConvert.SerializeObject(message));
        }

        public async Task PublishAsync(string topic, string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            try
            {
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

        public async Task PublishAsync(string topic, object message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            await PublishAsync(topic, JsonConvert.SerializeObject(message));
        }

        public async Task PublishAsync(string topic, IMessage message)
        {
            if (message == null) 
            { throw new ArgumentNullException(nameof(message)); }

            if(message.Id == null)
            { message.Id = IdGenerator.GetNewId(); }

            await PublishAsync(topic, JsonConvert.SerializeObject(message));
        }
    }
}