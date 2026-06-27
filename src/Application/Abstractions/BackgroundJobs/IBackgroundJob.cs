using System.Linq.Expressions;

namespace Application.Abstractions.BackgroundJobs;

public interface IBackgroundJob
{
    /// <summary>
    /// Enqueues a job to be executed in the background
    /// </summary>
    /// <typeparam name="T">The type of the job</typeparam>
    /// <param name="methodCall">The method to execute</param>
    /// <returns>Job identifier</returns>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Schedules a job to be executed after a specified delay
    /// </summary>
    /// <typeparam name="T">The type of the job</typeparam>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <returns>Job identifier</returns>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedules a job to be executed at a specific time
    /// </summary>
    /// <typeparam name="T">The type of the job</typeparam>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">Time to execute the job</param>
    /// <returns>Job identifier</returns>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
}
