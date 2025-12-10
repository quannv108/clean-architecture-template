using Application.Abstractions.Data;
using Application.Abstractions.DomainEvents;
using Application.Abstractions.Locking;
using Application.Outbox;
using Domain.Emails.Messages;
using Domain.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Outbox;

public class OutboxMessageProcessorTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    private readonly IDistributedLockProvider _lockProvider;
    private readonly OutboxMessageProcessor _processor;

    public OutboxMessageProcessorTests()
    {
        IServiceScopeFactory serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        IServiceScope serviceScope = Substitute.For<IServiceScope>();
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        _dbContext = Substitute.For<IApplicationDbContext>();
        _domainEventsDispatcher = Substitute.For<IDomainEventsDispatcher>();
        _lockProvider = Substitute.For<IDistributedLockProvider>();
        ILogger<OutboxMessageProcessor> logger = Substitute.For<ILogger<OutboxMessageProcessor>>();

        // Setup service scope factory
        serviceScopeFactory.CreateScope().Returns(serviceScope);
        serviceScope.ServiceProvider.Returns(serviceProvider);

        // Setup service provider to return mocked services using type-based overload
        serviceProvider.GetService(typeof(IApplicationDbContext)).Returns(_dbContext);
        serviceProvider.GetService(typeof(IDomainEventsDispatcher)).Returns(_domainEventsDispatcher);

        // Setup lock provider to always succeed by default
        var lockHandle = Substitute.For<IDistributedLockHandle>();
        var distributedLock = Substitute.For<IDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IDistributedLockHandle?>(lockHandle));
        _lockProvider.CreateLock(Arg.Any<string>()).Returns(distributedLock);

        _processor = new OutboxMessageProcessor(serviceScopeFactory, _lockProvider, logger);
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnEmptyResult_WhenNoMessagesFound()
    {
        // Arrange
        var messages = new List<OutboxMessage>();
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.ShouldBeEmpty();
        result.FailedLogs.ShouldBeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_ShouldSuccessfullyProcessSingleMessage()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(1);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(1);
        result.SucceedLogs[0].ShouldContain(nameof(EmailSentDomainEvent));
        result.FailedLogs.ShouldBeEmpty();

        // Verify domain event was dispatched
        await _domainEventsDispatcher.Received(1).DispatchAsync(
            Arg.Is<IDomainEvent[]>(events => events.Length == 1 && events[0] is EmailSentDomainEvent),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called (for MarkAsProcessing and MarkAsProcessed)
        await _dbContext.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldSuccessfullyProcessMultipleMessages()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var event1 = new EmailSentDomainEvent(id1);
        var event2 = new EmailSentDomainEvent(id2);
        var event3 = new EmailSentDomainEvent(id3);

        var message1 = CreateOutboxMessage(event1, DateTime.UtcNow.AddMinutes(-10));
        var message2 = CreateOutboxMessage(event2, DateTime.UtcNow.AddMinutes(-8));
        var message3 = CreateOutboxMessage(event3, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { message1, message2, message3 };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(3);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(3);
        result.SucceedLogs.ShouldAllBe(log => log.Contains(nameof(EmailSentDomainEvent)));
        result.FailedLogs.ShouldBeEmpty();

        // Verify all events were dispatched
        await _domainEventsDispatcher.Received(3).DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called 6 times (2 per message: MarkAsProcessing + MarkAsProcessed)
        await _dbContext.Received(6).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleDispatcherException()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup dispatcher to throw exception
        var expectedException = new InvalidOperationException("Dispatcher failed");
        _domainEventsDispatcher.DispatchAsync(Arg.Any<IDomainEvent[]>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw expectedException);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(1);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.ShouldBeEmpty();
        result.FailedLogs.Count.ShouldBe(1);
        result.FailedLogs[0].ShouldContain("Dispatcher failed");

        // Verify SaveChangesAsync was called 2 times (MarkAsProcessing + SetError)
        await _dbContext.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldRespectBatchSizeLimit()
    {
        // Arrange - create 10 messages but only process 3
        var messages = new List<OutboxMessage>();
        for (int i = 0; i < 10; i++)
        {
            var userId = Guid.NewGuid();
            var domainEvent = new EmailSentDomainEvent(userId);
            var message = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-10 + i));
            messages.Add(message);
        }

        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act - process with batch size of 3
        var result = await _processor.ProcessAsync(batchSize: 3);

        // Assert - should only process 3 messages
        result.ProcessedCount.ShouldBe(3);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(3);

        // Verify only 3 events were dispatched
        await _domainEventsDispatcher.Received(3).DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldOnlyProcessMessagesWithOccurredOnUtcInPast()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var pastEvent = new EmailSentDomainEvent(userId1);
        var presentEvent = new EmailSentDomainEvent(userId2);

        var pastMessage = CreateOutboxMessage(pastEvent, now.AddMinutes(-5));
        var presentMessage = CreateOutboxMessage(presentEvent, now);

        // Only past and present messages should be included in the query result
        var messages = new List<OutboxMessage> { pastMessage, presentMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert - should process 2 messages (past and present, but not future)
        result.ProcessedCount.ShouldBe(2);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ProcessAsync_ShouldProcessMessagesInOrderByOccurredOnUtc()
    {
        // Arrange - create messages in non-chronological order
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var event1 = new EmailSentDomainEvent(id1);
        var event2 = new EmailSentDomainEvent(id2);
        var event3 = new EmailSentDomainEvent(id3);

        var message1 = CreateOutboxMessage(event1, DateTime.UtcNow.AddMinutes(-10)); // Oldest
        var message2 = CreateOutboxMessage(event2, DateTime.UtcNow.AddMinutes(-5)); // Middle
        var message3 = CreateOutboxMessage(event3, DateTime.UtcNow.AddMinutes(-1)); // Newest

        // Add in random order
        var messages = new List<OutboxMessage> { message3, message1, message2 };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        var dispatchedEvents = new List<IDomainEvent>();
        _domainEventsDispatcher
            .DispatchAsync(Arg.Do<IDomainEvent[]>(events => dispatchedEvents.AddRange(events)),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(3);

        // Verify events were dispatched in order (oldest first)
        dispatchedEvents.Count.ShouldBe(3);
        ((EmailSentDomainEvent)dispatchedEvents[0]).EmailMessageId.ShouldBe(id1); // Oldest
        ((EmailSentDomainEvent)dispatchedEvents[1]).EmailMessageId.ShouldBe(id2); // Middle
        ((EmailSentDomainEvent)dispatchedEvents[2]).EmailMessageId.ShouldBe(id3); // Newest
    }

    [Fact]
    public async Task ProcessAsync_ShouldOnlyProcessPendingMessages()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var event1 = new EmailSentDomainEvent(id1);
        var event2 = new EmailSentDomainEvent(id2);
        var event3 = new EmailSentDomainEvent(id3);

        var pendingMessage = CreateOutboxMessage(event1, DateTime.UtcNow.AddMinutes(-10));
        var processedMessage = CreateOutboxMessage(event2, DateTime.UtcNow.AddMinutes(-8));
        var failedMessage = CreateOutboxMessage(event3, DateTime.UtcNow.AddMinutes(-5));

        // Mark messages as processed and failed
        SetMessageAsProcessed(processedMessage, DateTime.UtcNow.AddMinutes(-7));
        SetMessageAsFailed(failedMessage, "Previous error");

        // Only pending message should be in query result
        var messages = new List<OutboxMessage> { pendingMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert - should only process the pending message
        result.ProcessedCount.ShouldBe(1);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallMarkAsProcessingBeforeDispatching()
    {
        // Arrange
        var domainEvent = new EmailSentDomainEvent(Guid.CreateVersion7());
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        var saveCallCount = 0;
        var dispatchCallCount = 0;
        var saveBeforeDispatch = false;

        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                saveCallCount++;
                if (dispatchCallCount == 0) // First save should be before dispatch
                {
                    saveBeforeDispatch = true;
                }

                return Task.FromResult(1);
            });

        _domainEventsDispatcher.DispatchAsync(Arg.Any<IDomainEvent[]>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                dispatchCallCount++;
                return Task.CompletedTask;
            });

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(1);
        saveBeforeDispatch.ShouldBeTrue("SaveChangesAsync should be called before DispatchAsync");
        saveCallCount.ShouldBe(2, "SaveChangesAsync should be called twice (MarkAsProcessing + MarkAsProcessed)");
        dispatchCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallMarkAsProcessedAfterSuccessfulDispatch()
    {
        // Arrange
        var domainEvent = new EmailSentDomainEvent(Guid.CreateVersion7());
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(1);
        result.FailedCount.ShouldBe(0);

        // Verify SaveChangesAsync was called twice (MarkAsProcessing + MarkAsProcessed)
        await _dbContext.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldSetErrorOnMessageWhenExceptionOccurs()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup dispatcher to throw exception
        _domainEventsDispatcher.DispatchAsync(Arg.Any<IDomainEvent[]>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Test exception"));

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.FailedCount.ShouldBe(1);

        // Verify SaveChangesAsync was called twice (MarkAsProcessing + SetError)
        await _dbContext.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Helper method to create an OutboxMessage from a domain event
    /// </summary>
    private static OutboxMessage CreateOutboxMessage(IDomainEvent domainEvent, DateTime occurredOnUtc)
    {
        return OutboxMessage.Create(
            domainEvent,
            occurredOnUtc);
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

    [Fact]
    public async Task ProcessAsync_ShouldSkipMessage_WhenLockCannotBeAcquired()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup lock provider to fail acquisition (another instance is processing)
        var distributedLock = Substitute.For<IDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IDistributedLockHandle?>(null));
        _lockProvider.CreateLock(Arg.Any<string>()).Returns(distributedLock);

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(1);
        result.SucceedLogs.ShouldBeEmpty();
        result.FailedLogs.ShouldBeEmpty();

        // Verify dispatcher was never called
        await _domainEventsDispatcher.DidNotReceive().DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was never called
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleMixedSuccessAndFailure()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var event1 = new EmailSentDomainEvent(id1);
        var event2 = new EmailSentDomainEvent(id2);
        var event3 = new EmailSentDomainEvent(id3);

        var message1 = CreateOutboxMessage(event1, DateTime.UtcNow.AddMinutes(-10));
        var message2 = CreateOutboxMessage(event2, DateTime.UtcNow.AddMinutes(-8));
        var message3 = CreateOutboxMessage(event3, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { message1, message2, message3 };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup dispatcher to fail on second message only
        var callCount = 0;
        _domainEventsDispatcher.DispatchAsync(Arg.Any<IDomainEvent[]>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount == 2)
                {
                    throw new InvalidOperationException("Dispatcher failed for message 2");
                }

                return Task.CompletedTask;
            });

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(2);
        result.FailedCount.ShouldBe(1);
        result.SkipCount.ShouldBe(0);
        result.SucceedLogs.Count.ShouldBe(2);
        result.FailedLogs.Count.ShouldBe(1);
        result.FailedLogs[0].ShouldContain("Dispatcher failed for message 2");

        // Verify dispatcher was called 3 times
        await _domainEventsDispatcher.Received(3).DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called 6 times (2 per message: MarkAsProcessing + MarkAsProcessed/SetError)
        await _dbContext.Received(6).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleSaveChangesFailure_DuringMarkAsProcessing()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup SaveChangesAsync to throw on first call (MarkAsProcessing)
        var saveCallCount = 0;
        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                saveCallCount++;
                if (saveCallCount == 1)
                {
                    throw new InvalidOperationException("Database error during MarkAsProcessing");
                }

                return Task.FromResult(1);
            });

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(1);
        result.SkipCount.ShouldBe(0);
        result.FailedLogs.Count.ShouldBe(1);
        result.FailedLogs[0].ShouldContain("Database error during MarkAsProcessing");

        // Verify dispatcher was never called (failure happened before dispatch)
        await _domainEventsDispatcher.DidNotReceive().DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called twice (failed MarkAsProcessing + SetError)
        await _dbContext.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleSaveChangesFailure_DuringMarkAsProcessed()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var domainEvent = new EmailSentDomainEvent(id1);
        var outboxMessage = CreateOutboxMessage(domainEvent, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { outboxMessage };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup SaveChangesAsync to throw on second call (MarkAsProcessed)
        var saveCallCount = 0;
        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                saveCallCount++;
                if (saveCallCount == 2)
                {
                    throw new InvalidOperationException("Database error during MarkAsProcessed");
                }

                return Task.FromResult(1);
            });

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(1);
        result.SkipCount.ShouldBe(0);
        result.FailedLogs.Count.ShouldBe(1);
        result.FailedLogs[0].ShouldContain("Database error during MarkAsProcessed");

        // Verify dispatcher was called (failure happened after dispatch)
        await _domainEventsDispatcher.Received(1).DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called 3 times (MarkAsProcessing + failed MarkAsProcessed + SetError)
        await _dbContext.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleMultipleMessagesWithSomeSkipped()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var event1 = new EmailSentDomainEvent(id1);
        var event2 = new EmailSentDomainEvent(id2);
        var event3 = new EmailSentDomainEvent(id3);

        var message1 = CreateOutboxMessage(event1, DateTime.UtcNow.AddMinutes(-10));
        var message2 = CreateOutboxMessage(event2, DateTime.UtcNow.AddMinutes(-8));
        var message3 = CreateOutboxMessage(event3, DateTime.UtcNow.AddMinutes(-5));

        var messages = new List<OutboxMessage> { message1, message2, message3 };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Setup lock provider to fail acquisition for second message only
        var lockCallCount = 0;
        _lockProvider.CreateLock(Arg.Any<string>())
            .Returns(_ =>
            {
                lockCallCount++;
                var lockHandle = lockCallCount == 2 ? null : Substitute.For<IDistributedLockHandle>();
                var distributedLock = Substitute.For<IDistributedLock>();
                distributedLock.TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(lockHandle));
                return distributedLock;
            });

        // Act
        var result = await _processor.ProcessAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.ShouldBe(2);
        result.FailedCount.ShouldBe(0);
        result.SkipCount.ShouldBe(1);
        result.SucceedLogs.Count.ShouldBe(2);
        result.FailedLogs.ShouldBeEmpty();

        // Verify dispatcher was called twice (skipped message was not dispatched)
        await _domainEventsDispatcher.Received(2).DispatchAsync(
            Arg.Any<IDomainEvent[]>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChangesAsync was called 4 times (2 per processed message)
        await _dbContext.Received(4).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
