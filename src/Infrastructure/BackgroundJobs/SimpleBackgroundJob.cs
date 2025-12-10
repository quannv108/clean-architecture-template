using Application.Abstractions.BackgroundJobs;

namespace Infrastructure.BackgroundJobs;

internal sealed class SimpleBackgroundJob : IBackgroundJob
{
    public string Enqueue<T>(System.Linq.Expressions.Expression<System.Func<T, Task>> methodCall)
    {
        // TODO: Implement with chosen background job library
        // For now, execute immediately (not ideal for production)
        var jobId = Guid.NewGuid().ToString();

        // In a real implementation, this would be queued for background execution
        // Currently just returns a job ID without actual background processing

        return jobId;
    }

    public string Schedule<T>(System.Linq.Expressions.Expression<System.Func<T, Task>> methodCall, TimeSpan delay)
    {
        // TODO: Implement with chosen background job library
        var jobId = Guid.NewGuid().ToString();

        // In a real implementation, this would be scheduled for future execution

        return jobId;
    }

    public string Schedule<T>(System.Linq.Expressions.Expression<System.Func<T, Task>> methodCall,
        DateTimeOffset enqueueAt)
    {
        // TODO: Implement with chosen background job library
        var jobId = Guid.NewGuid().ToString();

        // In a real implementation, this would be scheduled for specific time execution

        return jobId;
    }
}
