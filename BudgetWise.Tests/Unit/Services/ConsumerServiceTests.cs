using BudgetWise.BLL.Interfaces;
using BudgetWise.BLL.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace BudgetWise.Tests.Unit.Services;

public class ConsumerServiceTests
{
    [Fact]
    public async Task StartAsync_ProcessesMessage_WhenValidMessageReceived()
    {
        // Arrange
        var topic = "transactions";
        var groupId = "group-1";

        var mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
        var mockConsumer = new Mock<IConsumer<Ignore, string>>();

        var message = CreateTestMessage();

        var consumeResult = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = JsonSerializer.Serialize(message)
            }
        };

        mockConsumer
            .SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult)
            .Throws(new OperationCanceledException());

        mockConsumerFactory
            .Setup(x => x.Create(It.IsAny<ConsumerConfig>()))
            .Returns(mockConsumer.Object);

        var service = CreateConsumerService(
            //mockConfiguration,
            mockConsumerFactory);

        TestMessage? handledMessage = null;

        // Act
        await service.StartAsync(
            topic,
            groupId,
            m =>
            {
                handledMessage = m;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // Assert
        Assert.NotNull(handledMessage);
        Assert.Equal(message.Id, handledMessage!.Id);
        Assert.Equal(message.Name, handledMessage.Name);

        mockConsumer.Verify(
            x => x.Subscribe(topic),
            Times.Once);

        mockConsumer.Verify(
            x => x.Commit(It.IsAny<ConsumeResult<Ignore, string>>()),
            Times.Once);

        mockConsumer.Verify(
            x => x.Close(),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_DoesNotCallHandler_WhenPayloadIsNull()
    {
        // Arrange
        var topic = "transactions";
        var groupId = "group-1";

        var mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
        var mockConsumer = new Mock<IConsumer<Ignore, string>>();

        var consumeResult = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = null!
            }
        };

        mockConsumer
            .SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult)
            .Throws(new OperationCanceledException());

        mockConsumerFactory
            .Setup(x => x.Create(It.IsAny<ConsumerConfig>()))
            .Returns(mockConsumer.Object);

        var service = CreateConsumerService(
            //mockConfiguration,
            mockConsumerFactory);

        var handlerCalled = false;

        // Act
        await service.StartAsync(
            topic,
            groupId,
            _ =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // Assert
        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task StartAsync_DoesNotCallHandler_WhenJsonIsInvalid()
    {
        // Arrange
        var topic = "transactions";
        var groupId = "group-1";

        var mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
        var mockConsumer = new Mock<IConsumer<Ignore, string>>();

        var consumeResult = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = "{ invalid json }"
            }
        };

        mockConsumer
            .SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult)
            .Throws(new OperationCanceledException());

        mockConsumerFactory
            .Setup(x => x.Create(It.IsAny<ConsumerConfig>()))
            .Returns(mockConsumer.Object);

        var service = CreateConsumerService(
            //mockConfiguration,
            mockConsumerFactory);

        var handlerCalled = false;

        // Act
        await service.StartAsync(
            topic,
            groupId,
            _ =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // Assert
        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task StartAsync_DoesNotCallHandler_WhenDeserializedMessageIsNull()
    {
        // Arrange
        var topic = "transactions";
        var groupId = "group-1";

        var mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
        var mockConsumer = new Mock<IConsumer<Ignore, string>>();

        var consumeResult = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = "null"
            }
        };

        mockConsumer
            .SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult)
            .Throws(new OperationCanceledException());

        mockConsumerFactory
            .Setup(x => x.Create(It.IsAny<ConsumerConfig>()))
            .Returns(mockConsumer.Object);

        var service = CreateConsumerService(
            //mockConfiguration,
            mockConsumerFactory);

        var handlerCalled = false;

        // Act
        await service.StartAsync(
            topic,
            groupId,
            _ =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // Assert
        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task StartAsync_ClosesConsumer_WhenCancellationOccurs()
    {
        // Arrange
        var topic = "transactions";
        var groupId = "group-1";

        var mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
        var mockConsumer = new Mock<IConsumer<Ignore, string>>();

        mockConsumer
            .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Throws(new OperationCanceledException());

        mockConsumerFactory
            .Setup(x => x.Create(It.IsAny<ConsumerConfig>()))
            .Returns(mockConsumer.Object);

        var service = CreateConsumerService(
            mockConsumerFactory);

        // Act
        await service.StartAsync(
            topic,
            groupId,
            _ => Task.CompletedTask,
            CancellationToken.None);

        // Assert
        mockConsumer.Verify(
            x => x.Close(),
            Times.Once);
    }

    // Helper Functions
    private static ConsumerService<TestMessage> CreateConsumerService(
        Mock<IKafkaConsumerFactory> consumerFactory)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:KafkaServer"] = "localhost:9092"
            })
            .Build();

        var mockLogger = new Mock<ILogger<ConsumerService<TestMessage>>>();

        return new ConsumerService<TestMessage>(
            configuration,
            consumerFactory.Object,
            mockLogger.Object);
    }

    private static TestMessage CreateTestMessage(
        int id = 1,
        string name = "Salary")
    {
        return new TestMessage
        {
            Id = id,
            Name = name
        };
    }

    public class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}