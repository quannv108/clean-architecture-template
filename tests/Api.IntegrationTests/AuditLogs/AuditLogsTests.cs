using System.Net;
using System.Text.Json;
using Api.IntegrationTests.Infrastructure;
using Application.Abstractions.Messaging;
using Application.AuditLogs;
using Domain;
using Domain.AuditLogs;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Api.IntegrationTests.AuditLogs;

[Collection(nameof(AuditLogsTests))]
public class AuditLogsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiClient _client;
    private readonly ApiTestFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditLogsTests(ApiTestFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        factory.TestOutputHelper = output;
        _client = new ApiClient(factory.CreateClient(), output);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetAuditLogs_AsAdmin_ShouldSucceed()
    {
        var systemTenant = SystemConstants.SystemTenantId;

        var response = await _client.GetAsync($"audit-logs?take=50&tenantId={systemTenant}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auditLogs = JsonSerializer.Deserialize<AuditLogListResponse>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);

        auditLogs.ShouldNotBeNull();
        auditLogs.AuditLogs.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteOldAuditLogs_ShouldDeleteLogsOlderThanRetentionDays()
    {
        // Arrange - Create a test user via API
        var email = "auditlog-test-" + Guid.NewGuid() + "@example.com";
        var password = "TestPassword123!";
        var registerRequest = new
        {
            email,
            firstName = "Audit",
            lastName = "Test",
            password
        };

        var registerResponse = await _client.PostAsync("users/register", registerRequest);
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var registerResult = JsonSerializer.Deserialize<JsonElement>(
            await registerResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var userId = registerResult.GetProperty("userId").GetGuid();

        // Get database context to create old audit logs
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Application.Abstractions.Data.IApplicationDbContext>();

        // Create old audit logs (older than 30 days)
        var oldDate = DateTime.UtcNow.AddDays(-35);
        var recentDate = DateTime.UtcNow.AddDays(-10);

        var oldLog1 = AuditLog.Create(userId, "OldAction1", oldDate, new Uri("http://example.com/1")).Value;
        var oldLog2 = AuditLog.Create(userId, "OldAction2", oldDate.AddDays(-5), new Uri("http://example.com/2")).Value;
        var recentLog = AuditLog.Create(userId, "RecentAction", recentDate, new Uri("http://example.com/3")).Value;

        await dbContext.AuditLogs.AddAsync(oldLog1);
        await dbContext.AuditLogs.AddAsync(oldLog2);
        await dbContext.AuditLogs.AddAsync(recentLog);
        await dbContext.SaveChangesAsync();

        var initialCount = dbContext.AuditLogs.Count();
        initialCount.ShouldBeGreaterThanOrEqualTo(3);

        // Act
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteOldAuditLogsCommand>>();
        var command = new DeleteOldAuditLogsCommand { RetentionDays = 30 };
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Refresh from database
        var finalCount = dbContext.AuditLogs.Count();
        finalCount.ShouldBeLessThan(initialCount);

        // Recent log should still exist
        var remainingLog = dbContext.AuditLogs.FirstOrDefault(al => al.Id == recentLog.Id);
        remainingLog.ShouldNotBeNull();

        // Old logs should be deleted
        var deletedLog1 = dbContext.AuditLogs.FirstOrDefault(al => al.Id == oldLog1.Id);
        var deletedLog2 = dbContext.AuditLogs.FirstOrDefault(al => al.Id == oldLog2.Id);
        deletedLog1.ShouldBeNull();
        deletedLog2.ShouldBeNull();
    }
}
