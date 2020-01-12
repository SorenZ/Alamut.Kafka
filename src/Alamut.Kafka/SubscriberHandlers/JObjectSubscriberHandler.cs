using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Kafka.Contracts;

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
        private readonly ConsumerConfig _config;
        private readonly SubscriberBinding _binding;


        public JObjectSubscriberHandler(IServiceProvider serviceProvider,
        ILogger<ISubscriberHandler> logger,
        ConsumerConfig config,
        SubscriberBinding binding)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _binding = binding;
        }

        public async Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token)
        {
            var isTopicHandlerAvailable = _binding.TopicHandlers.TryGetValue(result.Topic, out var handlerType);
            if (!isTopicHandlerAvailable)
            {
                _logger.LogWarning($"<{_config.GroupId}> received message on topic <{result.Topic}>, but there is no handler registered for topic.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = this.GetHandler(scope, handlerType);

                _logger.LogTrace($"<{_config.GroupId}> received message on topic <{result.Topic}>");

                var value = JObject.Parse(result.Value);

                await handler.Handle(value, token);
            }
        }

        private IJObjectMessageHandler GetHandler(IServiceScope scope, Type handlerType)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                var nullRefEx = new NullReferenceException(
                    $"<{_config.GroupId}> exception: no handler found for type <{handlerType}>");
                throw nullRefEx;
            }

            if (handler is IJObjectMessageHandler eventHandler)
            {
                return eventHandler;
            }

            var castEx = new InvalidCastException(
                $"<{_config.GroupId}> exception: handler <{handlerType}> not of type <{typeof(IJObjectMessageHandler)}>");
            throw castEx;
        }
    }
}