using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers.Strategy
{
    public class RetryHelperStaticStrategy : IRetryStrategy
    {
        public T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries, int baseDelay)
        {
            return RetryHelperStatic.ExecuteWithRetries(func, operationName, maxRetries, baseDelay);
        }

        public async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries, int baseDelay)
        {
            return await RetryHelperStatic.ExecuteWithRetriesAsync(funcAsync, operationName, maxRetries, baseDelay);
        }

        public void ExecuteWithRetries(Action action, string operationName, int maxRetries, int baseDelay)
        {
            RetryHelperStatic.ExecuteWithRetries(action, operationName, maxRetries, baseDelay);
        }

        public async Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries, int baseDelay)
        {
            await RetryHelperStatic.ExecuteWithRetriesAsync(actionAsync, operationName, maxRetries, baseDelay);
        }
    }
}
