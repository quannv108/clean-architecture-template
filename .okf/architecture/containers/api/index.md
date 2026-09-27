The containers of the .NET backend in `api/`.

# The production container

* [Web.Api Container](web-api.md) - The single deployed backend application: the HTTP API plus the outbox processor and recurring jobs, all in one process.

There is exactly one backend container; what follows from that is in [Web.Api Container](web-api.md).

# Data stores

* [PostgreSQL Database](postgres.md) - The application database, and the source of the xmin row version and advisory locks the architecture depends on.
* [Redis](redis.md) - Optional. One connection string turns on the shared L2 cache tier and the Redis distributed lock provider together.

# Development-time only

* [AppHost (development orchestrator)](apphost.md) - The .NET Aspire host that starts the API together with Postgres, pgweb and Seq for local development. Not deployed.
* [Seq](seq.md) - The structured log server in the local stack, at http://localhost:8081.
* [pgweb](pgweb.md) - A browser UI over the development database, started with the local stack.

None of these exist in a deployed environment.
