using Confluent.Kafka;

namespace BudgetWise.BLL.Interfaces
{
    public interface IConsumerService<T>
    {
        Task StartAsync(
            string topic,
            string groupId,
            Func<T, Task> handler,
            CancellationToken cancellationToken);
    }

    public interface IKafkaConsumerFactory
    {
        IConsumer<Ignore, string> Create(ConsumerConfig config);
    }
}
