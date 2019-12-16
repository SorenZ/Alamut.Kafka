using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Alamut.Kafka.SubscriberHandlers
{
    public class GenericSubscriberHandler:  ISubscriberHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly KafkaConfig _kafkaConfig;
        
        public GenericSubscriberHandler(IServiceProvider serviceProvider,
        ILogger<ISubscriberHandler> logger,
        KafkaConfig kafkaConfig)
        {
            _kafkaConfig = kafkaConfig;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token)
        {
            var isTopicHandlerAvailable = KafkaSubscriberRegistrationExtensions.TopicHandlers.TryGetValue(result.Topic, out var handlerType);
            if (!isTopicHandlerAvailable)
            {
                _logger.LogWarning($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>, but there is no handler registered for topic.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = this.GetHandler(scope, handlerType.Key);

                _logger.LogTrace($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>");

                dynamic value = JsonConvert.DeserializeObject(result.Value, handlerType.Value);

                await (Task) handler.Handle(value, (dynamic)token);
            }
        }

        private dynamic GetHandler(IServiceScope scope, Type handlerType)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                var nullRefEx = new NullReferenceException(
                    $"<{_kafkaConfig.GroupId}> exception: no handler found for type <{handlerType}>");
                throw nullRefEx;
            }

            return handler;

            // if (handler is IGenericSubscriber eventHandler)
            // {
            //     return eventHandler;
            // }

            // var castEx = new InvalidCastException(
            //     $"<{_kafkaConfig.GroupId}> exception: handler <{handlerType}> not of type <{typeof(IGenericSubscriber<>)}>");
            // throw castEx;
        }
    }
}