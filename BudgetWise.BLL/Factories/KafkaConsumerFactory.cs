using BudgetWise.BLL.Interfaces;
using Confluent.Kafka;

namespace BudgetWise.BLL.Factories
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        public IConsumer<Ignore, string> Create(
            ConsumerConfig config)
        {
            return new ConsumerBuilder<Ignore, string>(config)
                .Build();
        }
    }
}