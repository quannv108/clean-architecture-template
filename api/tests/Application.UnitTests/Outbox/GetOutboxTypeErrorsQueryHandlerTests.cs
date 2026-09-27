using Application.Abstractions.Data;
using Application.Outbox;
using Domain.Emails;
using Domain.Outbox;
using MockQueryable.NSubstitute;

namespace Application.UnitTests.Outbox;

public class GetOutboxTypeErrorsQueryHandlerTests
{
    private const string TargetType = "Domain.Emails.EmailSentDomainEvent";

    private readonly IReadOnlyApplicationDbContext _dbContext = Substitute.For<IReadOnlyApplicationDbContext>();
    private readonly GetOutboxTypeErrorsQueryHandler _handler;

    public GetOutboxTypeErrorsQueryHandlerTests()
    {
        _handler = new GetOutboxTypeErrorsQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnFailedMessages_OfRequestedType()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var failedTarget = CreateFailedMessage(now.AddMinutes(-5));
        var processedTarget = CreateMessage(now.AddMinutes(-4));
        processedTarget.MarkAsProcessing("m");
        processedTarget.MarkAsProcessed(now, "m");
        var failedOtherType = OutboxMessage.Create(new OtherDomainEvent(), now.AddMinutes(-3));
        failedOtherType.MarkAsProcessing("m");
        failedOtherType.SetError("other type error");

        var messages = new List<OutboxMessage> { failedTarget, processedTarget, failedOtherType };
        var messagesDbSet = messages.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxTypeErrorsQuery(TargetType), CancellationToken.None);

        // Assert
        var error = result.Value.Errors.ShouldHaveSingleItem();
        error.Id.ShouldBe(failedTarget.Id);
    }

    [Fact]
    public async Task Handle_ShouldOrderByOccurredOnUtcDescending_CappedAtTake()
    {
        // Arrange - seed 7 failures, expect the 5 newest, newest first
        var now = DateTime.UtcNow;
        var failures = Enumerable.Range(0, 7)
            .Select(i => CreateFailedMessage(now.AddMinutes(-i)))
            .ToList();
        // failures[0] is newest (offset 0), failures[6] is oldest (offset -6)
        var messagesDbSet = failures.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxTypeErrorsQuery(TargetType), CancellationToken.None);

        // Assert
        result.Value.Errors.Count.ShouldBe(5);
        result.Value.Errors.Select(e => e.Id).ShouldBe(failures.Take(5).Select(f => f.Id));
    }

    [Fact]
    public async Task Handle_ShouldIgnoreWindow_AndReturnOldFailures()
    {
        // Arrange
        var oldFailure = CreateFailedMessage(DateTime.UtcNow.AddYears(-2));
        var messagesDbSet = new List<OutboxMessage> { oldFailure }.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxTypeErrorsQuery(TargetType), CancellationToken.None);

        // Assert
        var error = result.Value.Errors.ShouldHaveSingleItem();
        error.Id.ShouldBe(oldFailure.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFailures()
    {
        // Arrange
        var processed = CreateMessage(DateTime.UtcNow);
        processed.MarkAsProcessing("m");
        processed.MarkAsProcessed(DateTime.UtcNow, "m");
        var messagesDbSet = new List<OutboxMessage> { processed }.BuildMockDbSet();
        _dbContext.OutboxMessages.Returns(messagesDbSet);

        // Act
        var result = await _handler.Handle(new GetOutboxTypeErrorsQuery(TargetType), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Errors.ShouldBeEmpty();
    }

    private static OutboxMessage CreateMessage(DateTime occurredOnUtc) =>
        OutboxMessage.Create(new EmailSentDomainEvent(Guid.CreateVersion7()), occurredOnUtc);

    private static OutboxMessage CreateFailedMessage(DateTime occurredOnUtc)
    {
        var message = CreateMessage(occurredOnUtc);
        message.MarkAsProcessing("test-machine");
        message.SetError("boom");
        return message;
    }

    private sealed record OtherDomainEvent : SharedKernel.IDomainEvent;
}
