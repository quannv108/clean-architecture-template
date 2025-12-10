using Application.Abstractions.Data;
using Application.Abstractions.Locking;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.UnitTests.Locking;

/// <summary>
/// Unit tests demonstrating how to mock IDistributedLockProvider in command handlers.
/// These tests ensure lock acquisition/failure scenarios work correctly.
/// </summary>
public sealed class DistributedLockProviderUnitTests
{
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IDistributedLockProvider _lockProvider = Substitute.For<IDistributedLockProvider>();
    private readonly IDistributedLock _distributedLock = Substitute.For<IDistributedLock>();

    [Fact]
    public async Task Handle_WhenLockAcquired_ShouldProcessSuccessfully()
    {
        // Arrange
        var lockHandle = Substitute.For<IDistributedLockHandle>();
        var resourceId = Guid.NewGuid();
        var lockName = $"test:resource:{resourceId}";

        _lockProvider.CreateLock(lockName).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(lockHandle); // Lock acquired successfully

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _lockProvider.Received(1).CreateLock(lockName);
        await _distributedLock.Received(1)
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockNotAcquired_ShouldReturnFailure()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var lockName = $"test:resource:{resourceId}";

        _lockProvider.CreateLock(lockName).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IDistributedLockHandle?)null); // Lock NOT acquired (another instance has it)

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Test.AlreadyBeingProcessed");
        result.Error.Description.ShouldBe(
            $"Resource with ID '{resourceId}' is already being processed by another instance.");
        _lockProvider.Received(1).CreateLock(lockName);
        await _distributedLock.Received(1)
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockAcquired_ShouldUseCorrectLockName()
    {
        // Arrange
        var lockHandle = Substitute.For<IDistributedLockHandle>();
        var resourceId = Guid.NewGuid();
        var expectedLockName = $"test:resource:{resourceId}";

        _lockProvider.CreateLock(expectedLockName).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(lockHandle);

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _lockProvider.Received(1).CreateLock(expectedLockName);
    }

    [Fact]
    public async Task Handle_WhenLockAcquired_ShouldUseCorrectTimeout()
    {
        // Arrange
        var lockHandle = Substitute.For<IDistributedLockHandle>();
        var resourceId = Guid.NewGuid();
        var expectedTimeout = TimeSpan.FromSeconds(30);

        _lockProvider.CreateLock(Arg.Any<string>()).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(expectedTimeout, Arg.Any<CancellationToken>())
            .Returns(lockHandle);

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _distributedLock.Received(1)
            .TryAcquireAsync(expectedTimeout, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockNotAcquired_ShouldNotSaveChanges()
    {
        // Arrange
        var resourceId = Guid.NewGuid();

        _lockProvider.CreateLock(Arg.Any<string>()).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IDistributedLockHandle?)null); // Lock NOT acquired

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockAcquired_ShouldSaveChanges()
    {
        // Arrange
        var lockHandle = Substitute.For<IDistributedLockHandle>();
        var resourceId = Guid.NewGuid();

        _lockProvider.CreateLock(Arg.Any<string>()).Returns(_distributedLock);
        _distributedLock
            .TryAcquireAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(lockHandle);
        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new TestCommandHandler(_dbContext, _lockProvider);
        var command = new TestCommand(resourceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// Test command and handler for demonstration purposes
internal sealed record TestCommand(Guid ResourceId) : ICommand;

internal sealed class TestCommandHandler(
    IApplicationDbContext context,
    IDistributedLockProvider lockProvider) : ICommandHandler<TestCommand>
{
    public async Task<Result> Handle(TestCommand command, CancellationToken cancellationToken)
    {
        // Create a lock specific to this resource to prevent concurrent processing
        var lockName = $"test:resource:{command.ResourceId}";

        await using var lockHandle = await lockProvider
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(30), cancellationToken);

        if (lockHandle is null)
        {
            // Another instance is already processing this resource
            return Result.Failure(new Error(
                "Test.AlreadyBeingProcessed",
                $"Resource with ID '{command.ResourceId}' is already being processed by another instance.",
                ErrorType.Conflict));
        }

        // Critical section - only one instance can execute this at a time
        // Simulate business logic
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
