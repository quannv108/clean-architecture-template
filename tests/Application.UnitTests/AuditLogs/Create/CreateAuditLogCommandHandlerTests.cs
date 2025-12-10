using Application.Abstractions.Data;
using Application.AuditLogs;
using Domain.AuditLogs;
using MockQueryable.NSubstitute;

namespace Application.UnitTests.AuditLogs.Create;

public class CreateAuditLogCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly CreateAuditLogCommandHandler _handler;

    public CreateAuditLogCommandHandlerTests()
    {
        _handler = new CreateAuditLogCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenAuditLogIsCreatedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var command = new CreateAuditLogCommand
        {
            UserId = userId,
            ActionName = "TestAction",
            ActionDateTime = DateTime.UtcNow,
            UrlPath = new Uri("http://example.com/test"),
            IpAddress = "192.168.1.1",
            HttpResponseCode = 200,
            AdditionalData = "Test data",
            TenantId = tenantId
        };

        var auditLogs = new List<AuditLog>().BuildMockDbSet();
        _dbContext.AuditLogs.Returns(auditLogs);

        // Mock SaveChangesAsync to return 1 (indicating one record was saved)
        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAuditLogCreationFailsDueToInvalidUserId()
    {
        // Arrange
        var command = new CreateAuditLogCommand
        {
            UserId = Guid.Empty, // Invalid user ID
            ActionName = "TestAction",
            ActionDateTime = DateTime.UtcNow,
            UrlPath = new Uri("http://example.com/test"),
            IpAddress = "192.168.1.1",
            HttpResponseCode = 200,
            AdditionalData = "Test data",
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Error.ShouldBe(AuditLogErrors.UserIdRequired());
        await _dbContext.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAuditLogCreationFailsDueToInvalidActionName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAuditLogCommand
        {
            UserId = userId,
            ActionName = "", // Empty action name
            ActionDateTime = DateTime.UtcNow,
            UrlPath = new Uri("http://example.com/test"),
            IpAddress = "192.168.1.1",
            HttpResponseCode = 200,
            AdditionalData = "Test data",
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Error.ShouldBe(AuditLogErrors.ActionNameRequired());
        await _dbContext.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAuditLogCreationFailsDueToInvalidActionDateTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAuditLogCommand
        {
            UserId = userId,
            ActionName = "TestAction",
            ActionDateTime = default, // Default date time
            UrlPath = new Uri("http://example.com/test"),
            IpAddress = "192.168.1.1",
            HttpResponseCode = 200,
            AdditionalData = "Test data",
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Error.ShouldBe(AuditLogErrors.ActionDateTimeRequired());
        await _dbContext.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAuditLogCreationFailsDueToInvalidUrlPath()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAuditLogCommand
        {
            UserId = userId,
            ActionName = "TestAction",
            ActionDateTime = DateTime.UtcNow,
            UrlPath = null!, // Null URL path
            IpAddress = "192.168.1.1",
            HttpResponseCode = 200,
            AdditionalData = "Test data",
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Error.ShouldBe(AuditLogErrors.UrlPathRequired());
        await _dbContext.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
