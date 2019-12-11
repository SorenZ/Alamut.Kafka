using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Alamut.Kafka
{
    public class KafkaDynamicService : KafkaService
    {
        public KafkaDynamicService(IServiceProvider serviceProvider, 
        ILogger<KafkaService> logger, 
        KafkaConfig kafkaConfig, 
        SubscriberHandler handler) : base(serviceProvider, logger, kafkaConfig, handler)
        {
        }

        override internal async Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token)
        {
            var isTopicHandlerAvailable = _handler.TopicHandlers.TryGetValue(result.Topic, out var handlerType);
            if (!isTopicHandlerAvailable)
            {
                _logger.LogWarning($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>, but there is no handler registered for topic.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = this.GetHandler(scope, handlerType);

                _logger.LogTrace($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>");

                dynamic value = JsonConvert.DeserializeObject(result.Value);

                await handler.Handle(value, token);
            }
        }

        new internal IDynamicSubscriber GetHandler(IServiceScope scope, Type handlerType)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                var nullRefEx = new NullReferenceException($"<{_kafkaConfig.GroupId}> exception: no handler found for type <{handlerType}>");
                throw nullRefEx;
            }

            if (handler is IDynamicSubscriber eventHandler)
            {
                return eventHandler;
            }

            var castEx = new InvalidCastException($"<{_kafkaConfig.GroupId}> exception: handler <{handlerType}> not of type <{typeof(IDynamicSubscriber)}>");
            throw castEx;
        }
    }
}