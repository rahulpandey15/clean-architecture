using System.Linq.Expressions;

namespace eCommerce.Application.Contracts;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Func<T, Task>> job);
    
    string Schedule<T>(Expression<Func<T, Task>> job, TimeSpan delay);
    
    void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> job, string cron);
}