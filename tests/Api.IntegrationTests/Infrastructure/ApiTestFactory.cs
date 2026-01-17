using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Communication.Email;
using Application.Abstractions.Communication.Sms;
using Application.Outbox;
using Domain.Emails.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Serilog;
using SharedKernel;
using SharedKernel.Common;
using SharedKernel.Extensions;
using Testcontainers.PostgreSql;
using Web.Api;
using Xunit.Abstractions;

namespace Api.IntegrationTests.Infrastructure;

/// <summary>
/// Test factory for integration tests.
/// IMPORTANT: Each test class using IClassFixture&lt;ApiTestFactory&gt; gets its own instance with:
/// - Fresh PostgreSQL container
/// - Fresh WebApplicationFactory (isolated hosted services)
/// - Fresh mock instances
/// This ensures complete isolation between test classes.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("cleanarchitecture_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public ITestOutputHelper TestOutputHelper { get; set; } = null!;

    public IEmailSender EmailSenderMock { get; } = Substitute.For<IEmailSender>();
    public ISmsSender SmsSenderMock { get; } = Substitute.For<ISmsSender>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure Serilog file logging for tests
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "test-.log");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Add Serilog file sink configuration for test environment
            var serilogConfig = new Dictionary<string, string?>
            {
                ["Serilog:WriteTo:1:Name"] = "File",
                ["Serilog:WriteTo:1:Args:path"] = logPath,
                ["Serilog:WriteTo:1:Args:rollingInterval"] = "Day",
                ["Serilog:WriteTo:1:Args:outputTemplate"] =
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                ["Serilog:MinimumLevel:Override:Microsoft"] = "Information"
            };
            config.AddInMemoryCollection(serilogConfig);
        });

        builder.ConfigureServices(services =>
        {
            services.Replace(new ServiceDescriptor(typeof(IEmailSender), EmailSenderMock));
            services.Replace(new ServiceDescriptor(typeof(ISmsSender), SmsSenderMock));
        });

        EmailSenderMock.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        SmsSenderMock.SendAsync(Arg.Any<PhoneNumber>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        builder.UseEnvironment("Testing");

        // Generate RSA key pair for JWT
        using var privateKey = System.Security.Cryptography.RSA.Create(2048);
        using var publicKey = System.Security.Cryptography.RSA.Create();
        publicKey.ImportRSAPublicKey(privateKey.ExportRSAPublicKey(), out _);

        // Export private key as PEM (PKCS#8 format)
        var rsaPrivateKeyPkcs8 = privateKey.ExportPkcs8PrivateKey();
        var rsaPem = new StringBuilder();
        rsaPem.AppendLine("-----BEGIN PRIVATE KEY-----");
        rsaPem.AppendLine(Convert.ToBase64String(rsaPrivateKeyPkcs8, Base64FormattingOptions.InsertLineBreaks));
        rsaPem.AppendLine("-----END PRIVATE KEY-----");

        // Export public key as PEM (X509 SubjectPublicKeyInfo format)
        var rsaPublicKeyX509 = publicKey.ExportSubjectPublicKeyInfo();
        var rsaPubPem = new StringBuilder();
        rsaPubPem.AppendLine("-----BEGIN PUBLIC KEY-----");
        rsaPubPem.AppendLine(Convert.ToBase64String(rsaPublicKeyX509, Base64FormattingOptions.InsertLineBreaks));
        rsaPubPem.AppendLine("-----END PUBLIC KEY-----");

        var encryptionKey = Convert.ToBase64String("this is a 256 bit key for AES256"u8.ToArray());
        const string dbNameKey = "main-read-write";
        var keyValuePairs = new List<KeyValuePair<string, string?>>
        {
            new($"ConnectionStrings:{dbNameKey}", _dbContainer.GetConnectionString()!),
            new("Encryption:Key", encryptionKey),
            new("Jwt:RsaPrivateKey", rsaPem.ToString()),
            new("Jwt:RsaPublicKey", rsaPubPem.ToString()),
            new("Jwt:Issuer", "Issuer1"),
            new("Jwt:Audience", "Audience1"),
            new("Jwt:TokenExpirationMinutes", "5"),
            new("Outbox:PollingIntervalMs", "1000"),
            new("Outbox:BatchSize", "10"),
        };

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddInMemoryCollection(keyValuePairs);

        var configRoot = configBuilder.Build();

        builder.UseConfiguration(configRoot);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await Log.CloseAndFlushAsync();
        await base.DisposeAsync();
    }

    public async Task ProcessDomainEventsAsync(string waitingDomainType)
    {
        int count = 0;
        var maxAttempts = 20;
        var attempts = 0;
        do
        {
            attempts++;
            if (attempts > maxAttempts)
            {
                TestOutputHelper.WriteLine("Can not find processed outbox message after {0} attempts.", maxAttempts);
                break;
            }

            // Simulate some delay for processing
            await Task.Delay(500);
            // Manually trigger the outbox message processing with limit 1 per process so we can test each event
            var outboxMessageProcessor = (IOutboxMessageProcessor)Services.GetService(typeof(IOutboxMessageProcessor))!;
            var processed = await outboxMessageProcessor.ProcessAsync(1, CancellationToken.None).ConfigureAwait(false);
            count = processed.ProcessedCount;
            // log to test context
            TestOutputHelper.WriteLine($"Processed {count} outbox messages" +
                                       (count != 0 ? $" : {processed.SucceedLogs.StringJoin()}" : string.Empty));
            if (processed.FailedCount != 0)
            {
                TestOutputHelper.WriteLine(
                    $"Failed {processed.FailedCount} outbox messages: {processed.FailedLogs.StringJoin()}");
            }

            if (!processed.SucceedLogs.Any(x => x.Contains(waitingDomainType, StringComparison.OrdinalIgnoreCase)))
            {
                count = 0; // manipulate to ensure we wait to get the right domain
            }
        } while (count == 0);
    }

    public object GetDbContext()
    {
        return Services.GetService(typeof(Application.Abstractions.Data.IApplicationDbContext))!;
    }
}

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;
    private const string ApiVersionPrefix = "api/v1/";

    public ApiClient(HttpClient httpClient, ITestOutputHelper output)
    {
        _httpClient = httpClient;
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static string BuildUrl(string endpoint)
    {
        // Don't add prefix if endpoint already starts with api/ or is a special endpoint like "health"
        if (endpoint.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ||
            endpoint.StartsWith("health", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        // Remove leading slash if present
        var path = endpoint.TrimStart('/');
        return $"{ApiVersionPrefix}{path}";
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var url = BuildUrl(endpoint);
        _output.WriteLine($"[HTTP GET] {url}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{content}\n");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        var url = BuildUrl(endpoint);
        _output.WriteLine($"[HTTP GET] {url}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{content}\n");
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var url = BuildUrl(endpoint);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        _output.WriteLine($"[HTTP POST] {url}\nBody: {json}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{responseBody}\n");
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        var url = BuildUrl(endpoint);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        _output.WriteLine($"[HTTP PUT] {url}\nBody: {json}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{responseBody}\n");
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var url = BuildUrl(endpoint);
        _output.WriteLine($"[HTTP DELETE] {url}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        var response = await _httpClient.DeleteAsync(url);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{responseBody}\n");
        return response;
    }

    public async Task<HttpResponseMessage> PatchAsync<T>(string endpoint, T data)
    {
        var url = BuildUrl(endpoint);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        _output.WriteLine($"[HTTP PATCH] {url}\nBody: {json}");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _output.WriteLine($"[HTTP HEADER] {header.Key}: {string.Join(", ", header.Value)}");
        }

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[HTTP RESPONSE] {url} - Status: {response.StatusCode}\n{responseBody}\n");
        return response;
    }
}
