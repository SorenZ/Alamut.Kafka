using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Kafka.Models;

using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;


namespace Alamut.Kafka.SubscriberHandlers
{
    /// <summary>
    /// provides a generics subscriber handler for Kafka consumer
    /// </summary>
    public class GenericSubscriberHandler:  ISubscriberHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly SubscriberBinding _binding;

        
        public GenericSubscriberHandler(IServiceProvider serviceProvider,
        ILogger<ISubscriberHandler> logger,
        SubscriberBinding binding)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _binding = binding;
        }

        public async Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token)
        {
            var isTopicHandlerAvailable = _binding.GenericTopicHandlers.TryGetValue(result.Topic, out var handlerType);
            if (!isTopicHandlerAvailable)
            {
                _logger.LogWarning($"received message on topic <{result.Topic}>, but there is no handler registered for topic.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = this.GetHandler(scope, handlerType.Key);

                _logger.LogTrace($"received message on topic <{result.Topic}>");

                dynamic value = JsonConvert.DeserializeObject(result.Value, handlerType.Value);
                
                await (Task) handler.Handle(value, (dynamic)token);    

                // if(value == null) // impossible to get null value here
                // { _logger.LogWarning($"could not cast value : {result.Value} to data structure type : {handlerType.Value}"); }
                // else
                // { await (Task) handler.Handle(value, (dynamic)token); }
            }
        }

        private dynamic GetHandler(IServiceScope scope, Type handlerType)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new NullReferenceException(
                    $"exception: no handler found for type <{handlerType}>");
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