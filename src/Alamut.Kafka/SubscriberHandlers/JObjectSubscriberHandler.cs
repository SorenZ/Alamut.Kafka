using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Alamut.Kafka.SubscriberHandlers
{
    public class JObjectSubscriberHandler:  ISubscriberHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly KafkaConfig _kafkaConfig;
        private readonly SubscriberBinding _binding;


        public JObjectSubscriberHandler(IServiceProvider serviceProvider,
        ILogger<ISubscriberHandler> logger,
        KafkaConfig kafkaConfig,
        SubscriberBinding binding)
        {
            _kafkaConfig = kafkaConfig;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _binding = binding;
        }

        public async Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token)
        {
            var isTopicHandlerAvailable = _binding.TopicHandlers.TryGetValue(result.Topic, out var handlerType);
            if (!isTopicHandlerAvailable)
            {
                _logger.LogWarning($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>, but there is no handler registered for topic.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = this.GetHandler(scope, handlerType);

                _logger.LogTrace($"<{_kafkaConfig.GroupId}> received message on topic <{result.Topic}>");

                var value = JObject.Parse(result.Value);

                await handler.Handle(value, token);
            }
        }

        private IJObjectSubscriber GetHandler(IServiceScope scope, Type handlerType)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                var nullRefEx = new NullReferenceException(
                    $"<{_kafkaConfig.GroupId}> exception: no handler found for type <{handlerType}>");
                throw nullRefEx;
            }

            if (handler is IJObjectSubscriber eventHandler)
            {
                return eventHandler;
            }

            var castEx = new InvalidCastException(
                $"<{_kafkaConfig.GroupId}> exception: handler <{handlerType}> not of type <{typeof(IJObjectSubscriber)}>");
            throw castEx;
        }
    }
}