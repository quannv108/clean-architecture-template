using System.Linq.Expressions;
using Application.Abstractions.BackgroundJobs;
using Hangfire;

namespace Infrastructure.BackgroundJobs.Hangfire;

internal sealed class HangfireBackgroundJob(IBackgroundJobClient backgroundJobClient) : IBackgroundJob
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return backgroundJobClient.Enqueue(methodCall);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return backgroundJobClient.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return backgroundJobClient.Schedule(methodCall, enqueueAt);
    }
}
