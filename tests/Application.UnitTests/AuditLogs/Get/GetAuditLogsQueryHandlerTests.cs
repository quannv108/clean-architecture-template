using Application.Abstractions.Data;
using Application.AuditLogs;
using Domain.AuditLogs;
using MockQueryable.NSubstitute;

namespace Application.UnitTests.AuditLogs.Get;

public class GetAuditLogsQueryHandlerTests
{
    private readonly IReadOnlyApplicationDbContext _dbContext = Substitute.For<IReadOnlyApplicationDbContext>();
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _handler = new GetAuditLogsQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogs_WhenThereAreAuditLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId, "Action 1", DateTime.UtcNow, new Uri("http://example.com/action1")).Value,
            AuditLog.Create(userId, "Action 2", DateTime.UtcNow.AddMinutes(-10), new Uri("http://example.com/action2"))
                .Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.AuditLogs.Count);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenThereAreNoAuditLogs()
    {
        // Arrange
        var auditLogs = new List<AuditLog>().BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.AuditLogs);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsWithUserIdFilter_WhenUserIdIsProvided()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId1, "Action 1", DateTime.UtcNow, new Uri("http://example.com/action1")).Value,
            AuditLog.Create(userId2, "Action 2", DateTime.UtcNow.AddMinutes(-10), new Uri("http://example.com/action2"))
                .Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            UserId = userId1,
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.AuditLogs);
        Assert.Equal(userId1, result.Value.AuditLogs[0].UserId);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsWithActionNameFilter_WhenActionNameIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId, "UserLogin", DateTime.UtcNow, new Uri("http://example.com/login")).Value,
            AuditLog.Create(userId, "UserLogout", DateTime.UtcNow.AddMinutes(-10), new Uri("http://example.com/logout"))
                .Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            ActionName = "Login",
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.AuditLogs);
        Assert.Contains("Login", result.Value.AuditLogs[0].ActionName);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsWithDateTimeFilters_WhenDateTimeFiltersAreProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId, "Action 1", baseTime.AddMinutes(-30), new Uri("http://example.com/action1")).Value,
            AuditLog.Create(userId, "Action 2", baseTime.AddMinutes(-10), new Uri("http://example.com/action2")).Value,
            AuditLog.Create(userId, "Action 3", baseTime.AddMinutes(10), new Uri("http://example.com/action3")).Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            FromDateTime = baseTime.AddMinutes(-20),
            ToDateTime = baseTime,
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.AuditLogs);
        Assert.Equal("Action 2", result.Value.AuditLogs[0].ActionName);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsWithPagination_WhenThereAreMoreRecordsThanTake()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>();
        for (int i = 0; i < 6; i++)
        {
            auditLogs.Add(AuditLog.Create(userId, $"Action {i}", DateTime.UtcNow.AddMinutes(-i),
                new Uri($"http://example.com/action{i}")).Value);
        }

        var mockAuditLogs = auditLogs.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(mockAuditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 5
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.AuditLogs.Count);
        Assert.True(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsOrderedByActionDateTimeDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId, "Action 1", baseTime.AddMinutes(-30), new Uri("http://example.com/action1")).Value,
            AuditLog.Create(userId, "Action 2", baseTime.AddMinutes(-10), new Uri("http://example.com/action2")).Value,
            AuditLog.Create(userId, "Action 3", baseTime.AddMinutes(-20), new Uri("http://example.com/action3")).Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.AuditLogs.Count);
        // Check that they are ordered by ActionDateTime descending
        Assert.True(result.Value.AuditLogs[0].ActionDateTime > result.Value.AuditLogs[1].ActionDateTime);
        Assert.True(result.Value.AuditLogs[1].ActionDateTime > result.Value.AuditLogs[2].ActionDateTime);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyWithHasMoreWhenTakeIsZeroAndThereAreAuditLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            AuditLog.Create(userId, "Action 1", DateTime.UtcNow, new Uri("http://example.com/action1")).Value
        }.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 0
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.AuditLogs);
        Assert.True(result.Value.HasMore); // HasMore should be true because there are audit logs but Take is 0
    }

    [Fact]
    public async Task Handle_ReturnsEmptyWithoutHasMoreWhenTakeIsZeroAndThereAreNoAuditLogs()
    {
        // Arrange
        var auditLogs = new List<AuditLog>().BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 0
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.AuditLogs);
        Assert.False(result.Value.HasMore); // HasMore should be false because there are no audit logs
    }

    [Fact]
    public async Task Handle_ReturnsAllWhenTakeIsLarge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>();
        for (int i = 0; i < 100; i++)
        {
            auditLogs.Add(AuditLog.Create(userId, $"Action {i}", DateTime.UtcNow.AddMinutes(-i),
                new Uri($"http://example.com/action{i}")).Value);
        }

        var mockAuditLogs = auditLogs.BuildMockDbSet();
        _dbContext.AuditLogs.Returns(mockAuditLogs);
        var query = new GetAuditLogsQuery
        {
            Take = 1000
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100, result.Value.AuditLogs.Count);
        Assert.False(result.Value.HasMore);
    }
}
