using BudgetWise.BLL.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BudgetWise.BLL.Services
{
    public class ConsumerService<T>(IConfiguration configuration, IKafkaConsumerFactory consumerFactory, ILogger<ConsumerService<T>> logger) : IConsumerService<T>
    {
        public async Task StartAsync(
            string topic,
            string groupId,
            Func<T, Task> handler,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Starting consumer for topic {Topic}. Kafka Server: {KafkaServer}",
                topic,
                configuration["ConnectionStrings:KafkaServer"]);

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["ConnectionStrings:KafkaServer"],
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SessionTimeoutMs = 45000,
                MaxPollIntervalMs = 300000
            };

            using var consumer = consumerFactory.Create(config);
            consumer.Subscribe(topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);
                        if (result?.Message?.Value is null) continue;

                        var message = JsonSerializer.Deserialize<T>(result.Message.Value);
                        if (message is null) continue;

                        await handler(message);
                        consumer.Commit(result);

                        logger.LogInformation(
                            "Processed message. Topic={Topic} Partition={Partition} Offset={Offset}",
                            result.Topic,
                            result.Partition.Value,
                            result.Offset.Value);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation(
                            "Consumer for topic {Topic} stopped.",
                            topic);
                        break;
                    }
                    catch (JsonException ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to deserialize message from topic {Topic}",
                            topic);
                    }
                    catch (ConsumeException ex)
                    {
                        logger.LogError(
                            ex,
                            "Kafka consume error on topic {Topic}",
                            topic);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to process message from topic {Topic}",
                            topic);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(
                    "Consumer for topic {Topic} stopped.",
                    topic);
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
