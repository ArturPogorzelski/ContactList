using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers.Strategy
{
    public interface IRetryStrategy
    {
        T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries, int baseDelay);
        Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries, int baseDelay);
        void ExecuteWithRetries(Action action, string operationName, int maxRetries, int baseDelay);
        Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries, int baseDelay);
    }
}
