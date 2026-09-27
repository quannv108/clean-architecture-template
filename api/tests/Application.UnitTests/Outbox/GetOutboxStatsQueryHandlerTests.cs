using Application.Abstractions.Data;
using Application.Outbox;
using Domain.Emails;
using Domain.Outbox;
using MockQueryable.NSubstitute;

namespace Application.UnitTests.Outbox;

public class GetOutboxStatsQueryHandlerTests
{
    private readonly IReadOnlyApplicationDbContext _dbContext = Substitute.For<IReadOnlyApplicationDbContext>();
    private readonly GetOutboxStatsQueryHandler _handler;

    public GetOutboxStatsQueryHandlerTests()
    {
        _handler = new GetOutboxStatsQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldFoldCountsPerStatus_IntoDictionary()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var messages = new List<OutboxMessage>
        {
            CreateMessage(now.AddMinutes(-10), OutboxMessageStatus.Pending),
            CreateMessage(now.AddMinutes(-9), OutboxMessageStatus.Processed),
            CreateMessage(now.AddMinutes(-8), OutboxMessageStatus.Processed),
            CreateMessage(now.AddMinutes(-7), OutboxMessageStatus.Failed)
        };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxStatsQuery(now.AddDays(-1)), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var stat = result.Value.Types.ShouldHaveSingleItem();
        stat.Type.ShouldBe(typeof(EmailSentDomainEvent).FullName);
        stat.Count(OutboxMessageStatus.Pending).ShouldBe(1);
        stat.Count(OutboxMessageStatus.Processed).ShouldBe(2);
        stat.Count(OutboxMessageStatus.Failed).ShouldBe(1);
        stat.Total.ShouldBe(4);
    }

    [Fact]
    public async Task Handle_SuccessRate_ShouldOnlyConsiderTerminalStatuses()
    {
        // Arrange - 3 Processed + 1 Failed + 2 Pending => rate based on 4 terminal (3/4 = 0.75)
        var now = DateTime.UtcNow;
        var messages = new List<OutboxMessage>
        {
            CreateMessage(now, OutboxMessageStatus.Processed),
            CreateMessage(now, OutboxMessageStatus.Processed),
            CreateMessage(now, OutboxMessageStatus.Processed),
            CreateMessage(now, OutboxMessageStatus.Failed),
            CreateMessage(now, OutboxMessageStatus.Pending),
            CreateMessage(now, OutboxMessageStatus.Pending)
        };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxStatsQuery(now.AddDays(-1)), CancellationToken.None);

        // Assert
        var stat = result.Value.Types.ShouldHaveSingleItem();
        stat.SuccessRate.ShouldBe(0.75);
    }

    [Fact]
    public async Task Handle_SuccessRate_ShouldBeZero_WhenOnlyPendingOrProcessing()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var messages = new List<OutboxMessage>
        {
            CreateMessage(now, OutboxMessageStatus.Pending),
            CreateMessage(now, OutboxMessageStatus.Processing)
        };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxStatsQuery(now.AddDays(-1)), CancellationToken.None);

        // Assert
        var stat = result.Value.Types.ShouldHaveSingleItem();
        stat.SuccessRate.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ShouldExcludeMessages_OlderThanSinceUtc()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var sinceUtc = now.AddHours(-1);
        var messages = new List<OutboxMessage>
        {
            CreateMessage(now.AddHours(-2), OutboxMessageStatus.Processed), // too old, excluded
            CreateMessage(now.AddMinutes(-30), OutboxMessageStatus.Processed) // within window
        };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxStatsQuery(sinceUtc), CancellationToken.None);

        // Assert
        var stat = result.Value.Types.ShouldHaveSingleItem();
        stat.Total.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMessages()
    {
        // Arrange
        var messagesDbSet = new List<OutboxMessage>().BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxStatsQuery(DateTime.UtcNow.AddDays(-1)), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Types.ShouldBeEmpty();
    }

    private static OutboxMessage CreateMessage(DateTime occurredOnUtc, OutboxMessageStatus status)
    {
        var message = OutboxMessage.Create(new EmailSentDomainEvent(Guid.CreateVersion7()), occurredOnUtc);

        if (status == OutboxMessageStatus.Processed)
        {
            message.MarkAsProcessing("test-machine");
            message.MarkAsProcessed(occurredOnUtc.AddSeconds(1), "test-machine");
        }
        else if (status == OutboxMessageStatus.Failed)
        {
            message.MarkAsProcessing("test-machine");
            message.SetError("boom");
        }
        else if (status == OutboxMessageStatus.Processing)
        {
            message.MarkAsProcessing("test-machine");
        }

        return message;
    }
}
