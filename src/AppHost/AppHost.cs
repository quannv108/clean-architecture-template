var builder = DistributedApplication.CreateBuilder(args);

var postgresUsername = builder.AddParameter("postgres-username", "postgres");
var postgresPassword = builder.AddParameter("postgres-password", "postgres");

var db = builder.AddPostgres("db")
    .WithImage("postgres:18-alpine")
    .WithUserName(postgresUsername)
    .WithPassword(postgresPassword)
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("main-read-write");

var seq = builder.AddSeq("seq")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("HOST_HTTP_PORT", "80")
    .WithEnvironment("HOST_INGESTION_PORT", "5341")
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(seq);

#pragma warning disable S6966
builder.Build().Run();
#pragma warning restore S6966
