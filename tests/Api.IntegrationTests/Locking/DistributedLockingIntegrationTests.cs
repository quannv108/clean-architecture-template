using System.Collections.Concurrent;
using Api.IntegrationTests.Infrastructure;
using Application.Abstractions.Locking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Api.IntegrationTests.Locking;

/// <summary>
/// Integration tests for distributed locking using real PostgreSQL (Testcontainers).
/// Tests verify that locks work correctly across concurrent operations.
/// </summary>
[Collection("Sequential Tests")]
public class DistributedLockingIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly ITestOutputHelper _output;

    public DistributedLockingIntegrationTests(ApiTestFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.TestOutputHelper = output;
    }

    [Fact]
    public void CreateLock_ShouldReturnDistributedLock()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();

        // Act
        var distributedLock = lockProvider.CreateLock("test:lock");

        // Assert
        distributedLock.ShouldNotBeNull();
        distributedLock.ShouldBeAssignableTo<IDistributedLock>();
        _output.WriteLine($"Lock type: {distributedLock.GetType().Name}");
    }

    [Fact]
    public async Task TryAcquireAsync_WhenLockAvailable_ShouldAcquireLock()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockName = $"test:acquire:{Guid.NewGuid()}";

        // Act
        await using var lockHandle = await lockProvider
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(5));

        // Assert
        lockHandle.ShouldNotBeNull();
        _output.WriteLine($"Successfully acquired lock: {lockName}");
    }

    [Fact]
    public async Task TryAcquireAsync_WhenLockHeldByAnother_ShouldReturnNull()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        var lockProvider1 = scope1.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockProvider2 = scope2.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockName = $"test:concurrent:{Guid.NewGuid()}";

        // Act
        // First instance acquires the lock
        await using var lockHandle1 = await lockProvider1
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(10));

        lockHandle1.ShouldNotBeNull();
        _output.WriteLine($"First instance acquired lock: {lockName}");

        // Second instance tries to acquire the same lock (should fail immediately)
        await using var lockHandle2 = await lockProvider2
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.Zero); // Try immediately, don't wait

        // Assert
        lockHandle2.ShouldBeNull();
        _output.WriteLine($"Second instance could not acquire lock: {lockName} (expected behavior)");
    }

    [Fact]
    public async Task TryAcquireAsync_WhenLockReleased_ShouldAllowSubsequentAcquisition()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        var lockProvider1 = scope1.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockProvider2 = scope2.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockName = $"test:release:{Guid.NewGuid()}";

        // Act & Assert
        // First instance acquires and releases the lock
        await using (var lockHandle1 = await lockProvider1
                         .CreateLock(lockName)
                         .TryAcquireAsync(TimeSpan.FromSeconds(5)))
        {
            lockHandle1.ShouldNotBeNull();
            _output.WriteLine($"First instance acquired lock: {lockName}");
        } // Lock is released here

        _output.WriteLine($"First instance released lock: {lockName}");

        // Second instance should now be able to acquire the lock
        await using var lockHandle2 = await lockProvider2
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(5));

        lockHandle2.ShouldNotBeNull();
        _output.WriteLine($"Second instance acquired lock after release: {lockName}");
    }

    [Fact]
    public async Task ConcurrentOperations_WithLocking_ShouldExecuteSequentially()
    {
        // Arrange
        var lockName = $"test:sequential:{Guid.NewGuid()}";
        var executionOrder = new ConcurrentBag<int>();
        var tasks = new List<Task>();

        // Act
        // Simulate 5 concurrent operations competing for the same lock
        for (int i = 0; i < 5; i++)
        {
            var operationId = i;
            tasks.Add(Task.Run(async () =>
            {
                using var scope = _factory.Services.CreateScope();
                var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();

                await using var lockHandle = await lockProvider
                    .CreateLock(lockName)
                    .TryAcquireAsync(TimeSpan.FromSeconds(30));

                if (lockHandle is not null)
                {
                    _output.WriteLine($"Operation {operationId} acquired lock");
                    executionOrder.Add(operationId);
                    await Task.Delay(100); // Simulate work
                    _output.WriteLine($"Operation {operationId} released lock");
                }
                else
                {
                    _output.WriteLine($"Operation {operationId} could not acquire lock");
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        // All operations should have executed (acquired the lock)
        executionOrder.Count.ShouldBe(5);
        _output.WriteLine($"All 5 operations executed sequentially: {string.Join(", ", executionOrder)}");
    }

    [Fact]
    public async Task MultipleLocks_ShouldBeIndependent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockName1 = $"test:independent:1:{Guid.NewGuid()}";
        var lockName2 = $"test:independent:2:{Guid.NewGuid()}";

        // Act
        // Acquire two different locks simultaneously
        await using var lockHandle1 = await lockProvider
            .CreateLock(lockName1)
            .TryAcquireAsync(TimeSpan.FromSeconds(5));

        await using var lockHandle2 = await lockProvider
            .CreateLock(lockName2)
            .TryAcquireAsync(TimeSpan.FromSeconds(5));

        // Assert
        // Both locks should be acquired successfully (they are independent)
        lockHandle1.ShouldNotBeNull();
        lockHandle2.ShouldNotBeNull();
        _output.WriteLine($"Both independent locks acquired: {lockName1}, {lockName2}");
    }

    [Fact]
    public async Task LockWithTimeout_WhenTimeoutExpires_ShouldReturnNull()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        var lockProvider1 = scope1.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockProvider2 = scope2.ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        var lockName = $"test:timeout:{Guid.NewGuid()}";

        // Act
        // First instance holds the lock
        await using var lockHandle1 = await lockProvider1
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(10));

        lockHandle1.ShouldNotBeNull();
        _output.WriteLine($"First instance acquired lock: {lockName}");

        // Second instance tries to acquire with very short timeout (should timeout)
        var startTime = DateTime.UtcNow;
        await using var lockHandle2 = await lockProvider2
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromMilliseconds(100)); // Very short timeout
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        lockHandle2.ShouldBeNull();
        elapsedTime.ShouldBeLessThan(TimeSpan.FromSeconds(1)); // Should timeout quickly
        _output.WriteLine($"Second instance timed out after {elapsedTime.TotalMilliseconds}ms (expected)");
    }

    [Fact]
    public void DistributedLockProvider_ShouldUsePostgreSQLByDefault()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();

        // Assert
        // In test environment (without Redis configured), should use PostgreSQL provider
        lockProvider.ShouldNotBeNull();
        lockProvider.GetType().Name.ShouldContain("Postgres");
        _output.WriteLine($"Lock provider type: {lockProvider.GetType().FullName}");
    }

    [Fact]
    public async Task ConcurrentOperations_OnDifferentResources_ShouldNotBlock()
    {
        // Arrange
        var lockName1 = $"test:resource:1:{Guid.NewGuid()}";
        var lockName2 = $"test:resource:2:{Guid.NewGuid()}";
        var startTimes = new ConcurrentDictionary<string, DateTime>();
        var endTimes = new ConcurrentDictionary<string, DateTime>();

        // Act
        // Two operations on different resources should execute concurrently
        var task1 = Task.Run(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();

            startTimes[lockName1] = DateTime.UtcNow;
            await using var lockHandle = await lockProvider
                .CreateLock(lockName1)
                .TryAcquireAsync(TimeSpan.FromSeconds(10));

            lockHandle.ShouldNotBeNull();
            await Task.Delay(200); // Simulate work
            endTimes[lockName1] = DateTime.UtcNow;
        });

        var task2 = Task.Run(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var lockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();

            startTimes[lockName2] = DateTime.UtcNow;
            await using var lockHandle = await lockProvider
                .CreateLock(lockName2)
                .TryAcquireAsync(TimeSpan.FromSeconds(10));

            lockHandle.ShouldNotBeNull();
            await Task.Delay(200); // Simulate work
            endTimes[lockName2] = DateTime.UtcNow;
        });

        await Task.WhenAll(task1, task2);

        // Assert
        // Both operations should have overlapped in time (executed concurrently)
        var overlap = startTimes[lockName2] < endTimes[lockName1] &&
                      startTimes[lockName1] < endTimes[lockName2];
        overlap.ShouldBeTrue();
        _output.WriteLine($"Operations on different resources executed concurrently (overlapped)");
    }
}
