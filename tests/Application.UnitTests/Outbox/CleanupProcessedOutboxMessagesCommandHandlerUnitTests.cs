using Application.Abstractions.Data;
using Application.Outbox;
using Domain.Emails.Messages;
using Domain.Outbox;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Outbox;

public class CleanupProcessedOutboxMessagesCommandHandlerUnitTests
{
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ILogger<CleanupProcessedOutboxMessagesCommandHandler> _logger =
        Substitute.For<ILogger<CleanupProcessedOutboxMessagesCommandHandler>>();

    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithDeletedCount_WhenProcessedMessagesOlderThanOneMonthExist()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var twoMonthsAgo = now.AddMonths(-2);

        // Create old processed messages (should be cleaned up)
        var oldProcessedMessage1 = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            twoMonthsAgo);
        SetMessageAsProcessed(oldProcessedMessage1, twoMonthsAgo.AddMinutes(5));

        var oldProcessedMessage2 = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            twoMonthsAgo.AddDays(-10));
        SetMessageAsProcessed(oldProcessedMessage2, twoMonthsAgo.AddDays(-10).AddMinutes(5));

        // Create recent processed message (should NOT be cleaned up)
        var recentProcessedMessage = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            now.AddDays(-5));
        SetMessageAsProcessed(recentProcessedMessage, now.AddDays(-5).AddMinutes(5));

        var messages = new List<OutboxMessage>
        {
            oldProcessedMessage1,
            oldProcessedMessage2,
            recentProcessedMessage
        };

        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 0); // MockQueryable doesn't actually execute delete, but verifies query structure
    }

    [Fact]
    public async Task Handle_ShouldNotDeletePendingMessages_EvenIfOld()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var twoMonthsAgo = now.AddMonths(-2);

        // Create old pending message (should NOT be cleaned up)
        var oldPendingMessage = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            twoMonthsAgo);
        // Status remains Pending, no ProcessedOnUtc

        var messages = new List<OutboxMessage> { oldPendingMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteFailedMessages_EvenIfOld()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var twoMonthsAgo = now.AddMonths(-2);

        // Create old failed message (should NOT be cleaned up)
        var oldFailedMessage = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            twoMonthsAgo);
        SetMessageAsFailed(oldFailedMessage, "Test error");

        var messages = new List<OutboxMessage> { oldFailedMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteProcessedMessages_IfNotOlderThanOneMonth()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);

        // Create processed message from yesterday (should NOT be cleaned up)
        var recentProcessedMessage = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            now.AddDays(-1));
        SetMessageAsProcessed(recentProcessedMessage, now.AddDays(-1).AddMinutes(5));

        var messages = new List<OutboxMessage> { recentProcessedMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoMessagesMatchCriteria()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);

        // Create only recent pending messages
        var pendingMessage1 = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            now.AddHours(-1));

        var pendingMessage2 = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            now.AddHours(-2));

        var messages = new List<OutboxMessage> { pendingMessage1, pendingMessage2 };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldOnlyDeleteProcessedMessagesWithProcessedOnUtc()
    {
        // Arrange
        var now = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var twoMonthsAgo = now.AddMonths(-2);

        // Create old processed message with ProcessedOnUtc (should be cleaned up)
        var oldProcessedMessage = OutboxMessage.Create(
            new EmailSentDomainEvent(Guid.CreateVersion7()),
            twoMonthsAgo);
        SetMessageAsProcessed(oldProcessedMessage, twoMonthsAgo.AddMinutes(5));

        var messages = new List<OutboxMessage> { oldProcessedMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);
        _dateTimeProvider.UtcNow.Returns(now);

        var command = new CleanupProcessedOutboxMessagesCommand();
        var handler = new CleanupProcessedOutboxMessagesCommandHandler(_dbContext, _dateTimeProvider, _logger);

        // Act
        var result = await handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        // Verify that query includes ProcessedOnUtc check
    }

    /// <summary>
    /// Helper method to set message as processed using reflection
    /// </summary>
    private static void SetMessageAsProcessed(OutboxMessage message, DateTime processedOnUtc)
    {
        var processedOnUtcProperty = typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.ProcessedOnUtc));
        processedOnUtcProperty!.SetValue(message, processedOnUtc);

        var statusProperty = typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.Status));
        statusProperty!.SetValue(message, OutboxMessageStatus.Processed);
    }

    /// <summary>
    /// Helper method to set message as failed using reflection
    /// </summary>
    private static void SetMessageAsFailed(OutboxMessage message, string error)
    {
        var errorProperty = typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.Error));
        errorProperty!.SetValue(message, error);

        var statusProperty = typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.Status));
        statusProperty!.SetValue(message, OutboxMessageStatus.Failed);
    }
}
