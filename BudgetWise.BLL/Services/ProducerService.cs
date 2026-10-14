using BudgetWise.BLL.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BudgetWise.BLL.Services
{
    public class ProducerService(
        IProducer<string, string> producer,
        ILogger<ProducerService> logger
        ) : IProducerService
    {
        public async Task PublishAsync<T>(
            string topic,
            T message,
            string? key = null,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var payload = JsonSerializer.Serialize(message);

                var result = await producer.ProduceAsync(
                    topic,
                    new Message<string, string>
                    {
                        Key = key!,
                        Value = payload
                    },
                    cancellationToken);

                logger.LogInformation(
                    "Key={Key} Topic={Topic} Partition={Partition} Offset={Offset}",
                    key,
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                logger.LogError(
                    ex,
                    "Failed to publish message to topic {Topic}",
                    topic);

                throw;
            }
        }
    }
}
