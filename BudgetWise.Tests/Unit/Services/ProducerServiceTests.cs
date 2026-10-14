using BudgetWise.BLL.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace BudgetWise.Tests.Unit.Services
{
    public class ProducerServiceTests
    {
        [Fact]
        public async Task PublishAsync_CallsProducer_WithCorrectTopicKeyAndMessage()
        {
            // Arrange
            var topic = "transactions";
            var key = "123";

            var mockProducer = new Mock<IProducer<string, string>>();
            var mockLogger = new Mock<ILogger<ProducerService>>();
            var producerService = CreateProducerService(mockProducer, mockLogger);

            var message = new TestMessage
            {
                Id = 1,
                Name = "Salary"
            };

            mockProducer
                .Setup(x => x.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<Message<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<string, string>
                {
                    Topic = topic,
                    Partition = new Partition(0),
                    Offset = new Offset(1)
                });

            var expectedPayload = JsonSerializer.Serialize(message);

            // Act
            await producerService.PublishAsync(topic, message, key);

            // Assert
            mockProducer.Verify(
                x => x.ProduceAsync(
                    topic,
                    It.Is<Message<string, string>>(m =>
                        m.Key == key &&
                        m.Value == expectedPayload),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_ThrowsProduceException_WhenKafkaFails()
        {
            // Arrange
            var topic = "transactions";

            var mockProducer = new Mock<IProducer<string, string>>();
            var mockLogger = new Mock<ILogger<ProducerService>>();
            var producerService = CreateProducerService(mockProducer, mockLogger);

            var exception = new ProduceException<string, string>(
                new Error(ErrorCode.Local_MsgTimedOut),
                new DeliveryResult<string, string>());

            mockProducer
                .Setup(x => x.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<Message<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<ProduceException<string, string>>(
                () => producerService.PublishAsync(
                    topic,
                    new TestMessage()));
        }

        // Helper Functions
        private static ProducerService CreateProducerService(
            Mock<IProducer<string, string>>? mockProducer = null,
            Mock<ILogger<ProducerService>>? mockLogger = null)
        {
            return new ProducerService(
                mockProducer!.Object,
                mockLogger!.Object);
        }

        private class TestMessage
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}