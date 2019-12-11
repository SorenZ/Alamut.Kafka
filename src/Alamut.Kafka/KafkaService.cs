using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Models;
using Alamut.Kafka.SubscriberHandlers;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka
{
    public class KafkaService : BackgroundService
    {
        internal const int commitPeriod = 5;
        internal readonly ILogger<KafkaService> _logger;
        internal readonly KafkaConfig _kafkaConfig;
        internal readonly ISubscriberHandler _handler;


        public KafkaService(ILogger<KafkaService> logger,
        KafkaConfig kafkaConfig,
        ISubscriberHandler handler)
        {
            _kafkaConfig = kafkaConfig;
            _logger = logger;
            _handler = handler;
        }


        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers,
                GroupId = _kafkaConfig.GroupId,
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };



            // Note: If a key or value deserializer is not set (as is the case below), the 
            // deserializer corresponding to the appropriate type from Confluent.Kafka.Deserializers
            // will be used automatically (where available). The default deserializer for string
            // is UTF8. The default deserializer for Ignore returns null for all input data
            // (including non-null data).
            var consumer = new ConsumerBuilder<Ignore, string>(config)
                // Note: All handlers are called on the main .Consume thread.
                .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                // .SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}]");
                    // possibly manually specify start offsets or override the partition assignment provided by
                    // the consumer group by returning a list of topic/partition/offsets to assign to, e.g.:
                    // 
                    // return partitions.Select(tp => new TopicPartitionOffset(tp, externalOffsets[tp]));
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    Console.WriteLine($"Revoking assignment: [{string.Join(", ", partitions)}]");
                })
                .Build();

            consumer.Subscribe(_kafkaConfig.Topics);

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
                            Console.WriteLine(
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
                                Console.WriteLine($"Commit error: {e.Error.Reason}");
                            }
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Closing consumer.");
                consumer.Close();
            }
        }
        
    }
}