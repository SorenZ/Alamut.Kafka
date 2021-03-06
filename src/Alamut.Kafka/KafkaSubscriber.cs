﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Kafka.Models;
using Alamut.Kafka.SubscriberHandlers;

using Confluent.Kafka;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka
{
    /// <summary>
    /// implements Kafka subscriber as a background service 
    /// it depends on KafkaConfig and ISubscriberHandler
    /// </summary>
    /// <remarks>https://github.com/confluentinc/confluent-kafka-dotnet</remarks>
    public class KafkaSubscriber : BackgroundService
    {
        private const int commitPeriod = 5;
        private readonly ILogger _logger;
        private readonly ConsumerConfig _config;
        private readonly ISubscriberHandler _handler;
        private readonly IEnumerable<string> _topics;

        public KafkaSubscriber(ILoggerFactory loggerFactory,
        ConsumerConfig config,
        ISubscriberHandler handler,
        IEnumerable<string> topics)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger(nameof(KafkaSubscriber) + "-" + config.GroupId);
            _handler = handler;
            _topics = topics;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {

            // Note: If a key or value deserializer is not set (as is the case below), the 
            // deserializer corresponding to the appropriate type from Confluent.Kafka.Deserializers
            // will be used automatically (where available). The default deserializer for string
            // is UTF8. The default deserializer for Ignore returns null for all input data
            // (including non-null data).
            var consumer = new ConsumerBuilder<Ignore, string>(_config)
                // Note: All handlers are called on the main .Consume thread.
                .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
                // .SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
                    // possibly manually specify start offsets or override the partition assignment provided by
                    // the consumer group by returning a list of topic/partition/offsets to assign to, e.g.:
                    // 
                    // return partitions.Select(tp => new TopicPartitionOffset(tp, externalOffsets[tp]));
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogInformation($"Revoking assignment: [{string.Join(", ", partitions)}]");
                })
                .Build();

            // consumer.Subscribe(_kafkaConfig.Topics);
            consumer.Subscribe(_topics);

            Task.Run(async () => await Consume(consumer, cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task Consume(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        if (consumeResult.IsPartitionEOF)
                        {
                            _logger.LogTrace(
                                $"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                            continue;
                        }

                        // Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Value}");
                        await _handler.HandleMessage(consumeResult, cancellationToken);

                        // if (consumeResult.Offset % commitPeriod == 0)
                        if (true)
                        {
                            // The Commit method sends a "commit offsets" request to the Kafka
                            // cluster and synchronously waits for the response. This is very
                            // slow compared to the rate at which the consumer is capable of
                            // consuming messages. A high performance application will typically
                            // commit offsets relatively infrequently and be designed handle
                            // duplicate messages in the event of failure.
                            try
                            {
                                consumer.Commit(consumeResult);
                            }
                            catch (KafkaException e)
                            {
                                _logger.LogError($"Commit error: {e.Error.Reason}");
                            }
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError($"Consume error: {e.Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error occurred on consumer handler Method.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Closing consumer.");
                consumer.Close();
            }
        }
        
    }
}