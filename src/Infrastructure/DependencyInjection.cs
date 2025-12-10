using Application.Abstractions.Authentication;
using Application.Abstractions.BackgroundJobs;
using Application.Abstractions.Communication.Email;
using Application.Abstractions.Communication.Sms;
using Application.Abstractions.Cryptography;
using Application.Abstractions.Data;
using Application.Abstractions.DomainEvents;
using Application.Abstractions.Locking;
using Infrastructure.Communication.Email;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Authentication;
using Infrastructure.BackgroundJobs;
using Infrastructure.BackgroundJobs.Hangfire;
using Infrastructure.Communication.Sms;
using Infrastructure.Cryptography;
using Infrastructure.Database;
using Infrastructure.Database.Interceptors;
using Infrastructure.Database.Seeding;
using Infrastructure.DomainEvents;
using Infrastructure.Locking;
using Infrastructure.Outbox;
using Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    private const string MainDatabaseConnectionKey = "main-read-write";
    private const string ReadOnlyDatabaseConnectionKey = "main-read-only";
    private const string HangfireDatabaseConnectionKey = "hangfire";

    private static readonly string[] PostgreSqlHealthCheckTags = ["db", "ready"];
    private static readonly string[] RedisHealthCheckTags = ["cache", "lock", "ready"];

    private static string? GetWriteConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString(MainDatabaseConnectionKey);

    private static string? GetReadConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString(ReadOnlyDatabaseConnectionKey);

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        return services
                .AddServices()
                .AddDatabase(configuration, logger)
                .AddHealthChecks(configuration, environment, logger)
                .AddCache(configuration, logger)
                .AddDistributedLocking(configuration, logger)
                .AddBackgroundJobs(configuration, environment)
                .AddEmailIntegration(configuration)
                .AddSmsIntegration(configuration, logger)
                .AddAuthenticationInternal()
            ;
    }

    private static IServiceCollection AddCache(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var redisConnection = configuration["Redis:ConnectionString"];
        var hasRedis = !string.IsNullOrWhiteSpace(redisConnection);

        if (hasRedis)
        {
            logger.LogInformation("Configuring HybridCache with Redis L2 cache at {RedisConnection}",
                redisConnection);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = configuration["Redis:InstanceName"] ?? "CleanArchitecture:";
            });
        }
        else
        {
            logger.LogInformation("Configuring HybridCache with L1 in-memory cache only (Redis not configured)");
        }

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024;
            options.MaximumKeyLength = 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(15),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });

        return services;
    }

    private static IServiceCollection AddDistributedLocking(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var redisConnection = configuration["Redis:ConnectionString"];
        var hasRedis = !string.IsNullOrWhiteSpace(redisConnection);

        if (hasRedis)
        {
            services.AddSingleton<IDistributedLockProvider>(sp =>
            {
                var connection = ConnectionMultiplexer.Connect(redisConnection!);
                return new RedisDistributedLockProvider(connection.GetDatabase());
            });

            logger.LogInformation("Distributed locks: Redis provider (high-performance mode)");
        }
        else
        {
            services.AddSingleton<IDistributedLockProvider>(_ =>
                new PostgresDistributedLockProvider(
                    GetWriteConnectionString(configuration)!));

            logger.LogInformation("Distributed locks: PostgreSQL advisory locks (using existing database)");
        }

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Configure and validate outbox options
        services.AddOptions<OutboxOptions>()
            .BindConfiguration("Outbox")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        // Configure and validate encryption options
        services.AddOptions<EncryptionOptions>()
            .BindConfiguration("Encryption")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<EncryptionOptions>, EncryptionOptionsValidator>();

        services.AddSingleton<IEncryptor, Encryptor>();
        services.AddScoped<IEntityDeleter, EntityDeleter>();

        return services;
    }

    private static string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return string.Empty;
        }

        // Mask password and other sensitive fields
        var masked = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Password|Pwd|User\s*Id|Username)\s*=\s*[^;]+",
            "$1=****",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return masked;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration,
        ILogger logger)
    {
        string? writeConnectionString = GetWriteConnectionString(configuration);
        string? readConnectionString = GetReadConnectionString(configuration);

        logger.LogInformation("Configuring write database with connection string: {ConnectionString}",
            MaskConnectionString(writeConnectionString));
        if (!string.IsNullOrWhiteSpace(readConnectionString))
        {
            logger.LogInformation("Configuring read database with connection string: {ConnectionString}",
                MaskConnectionString(readConnectionString));
        }

        // database interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<EntityIdGenerationInterceptor>();

        // database seeding
        services.AddTransient<DbSeeder>();

        // write database context
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) => options
            .UseNpgsql(writeConnectionString, npgsqlOptions =>
            {
                // Configure EF Core resilient execution for transient Npgsql failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, SchemaNameConstants.Default);
            })
            .UseSnakeCaseNamingConvention()
            .UseAsyncSeeding((context, _, _) =>
            {
                var dbSeeder = serviceProvider.GetRequiredService<DbSeeder>();
                return dbSeeder.SeedAsync(context);
            })
            .UseSeeding((context, _) => // this is required when using with synchronous `Migrate()` method
            {
                var dbSeeder = serviceProvider.GetRequiredService<DbSeeder>();
                dbSeeder.SeedAsync(context).GetAwaiter().GetResult();
            })
            .AddInterceptors(
                serviceProvider.GetRequiredService<EntityIdGenerationInterceptor>(),
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>()
            ));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        if (!string.IsNullOrWhiteSpace(readConnectionString))
        {
            // read-only database context configured to point to a read replica
            services.AddDbContext<ReadOnlyApplicationDbContext>(options => options
                .UseNpgsql(readConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName,
                        SchemaNameConstants.Default);
                })
                .UseSnakeCaseNamingConvention());

            services.AddScoped<IReadOnlyApplicationDbContext>(sp =>
                sp.GetRequiredService<ReadOnlyApplicationDbContext>());
        }
        else
        {
            // Fallback: use the write context for reads when no replica configured
            services.AddScoped<IReadOnlyApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        }

        return services;
    }

    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger logger)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return services;
        }

        var healthChecksBuilder = services.AddHealthChecks();

        // PostgreSQL health check (always present)
        healthChecksBuilder.AddNpgSql(
            GetWriteConnectionString(configuration)!,
            name: "postgresql",
            tags: PostgreSqlHealthCheckTags);

        // Redis health check (if configured)
        var redisConnection = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            healthChecksBuilder.AddRedis(
                redisConnection,
                name: "redis",
                tags: RedisHealthCheckTags);

            logger.LogInformation("Health checks: PostgreSQL + Redis");
        }
        else
        {
            logger.LogInformation("Health checks: PostgreSQL only");
        }

        return services;
    }
    
    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services
        )
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IHasher, Hasher>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Prefer dedicated Hangfire connection if provided; fallback to main database connection
        var hangfireConn = configuration.GetConnectionString(HangfireDatabaseConnectionKey);
        var mainConn = GetWriteConnectionString(configuration);
        var effectiveConn = string.IsNullOrWhiteSpace(hangfireConn) ? mainConn : hangfireConn;

        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(effectiveConn)));

        services.AddHangfireServer(options =>
        {
            options.ServerName = "CleanArchitecture.JobServer";
            options.WorkerCount = Environment.ProcessorCount * 2; // Adjust worker count based on your needs
            options.SchedulePollingInterval = TimeSpan.FromSeconds(10); // Adjust polling interval as needed
        });

        services.AddScoped<IBackgroundJob, HangfireBackgroundJob>();
        services.AddTransient<IRecurringJobConfigurator, HangfireRecurringJobConfigurator>();
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<OutboxMessageHostedService>();
        }

        return services;
    }

    private static IServiceCollection AddEmailIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure and validate email options
        services.AddOptions<EmailOptions>()
            .BindConfiguration("Email")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

        var provider = configuration["Email:Provider"];
        if (string.Equals(provider, "SES", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmailSender, SesEmailSender>();
        }
        else
        {
            // Default to dummy sender
            services.AddSingleton<IEmailSender, DummyEmailSender>();
        }

        return services;
    }

    private static IServiceCollection AddSmsIntegration(this IServiceCollection services, IConfiguration configuration,
        ILogger logger)
    {
        // Configure and validate SMS options
        services.AddOptions<SmsOptions>()
            .BindConfiguration("Sms")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SmsOptions>, SmsOptionsValidator>();

        var provider = configuration["Sms:Provider"];
        logger.LogInformation("SmsProvider = {SmsProvider}", provider);
        if (string.Equals(provider, "Twilio", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<ISmsSender, TwilioSmsSender>();
        }
        else
        {
            services.AddSingleton<ISmsSender, DummySmsSender>();
        }

        return services;
    }
}
