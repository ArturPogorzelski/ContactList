using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers.Strategy
{
    public class RetryHelperStrategy : IRetryStrategy
    {
        private readonly IRetryHelper _retryHelper;

        public RetryHelperStrategy(IRetryHelper retryHelper)
        {
            _retryHelper = retryHelper ?? throw new ArgumentNullException(nameof(retryHelper));
        }

        public T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries, int baseDelay)
        {
            return _retryHelper.ExecuteWithRetries(func, operationName, maxRetries, baseDelay);
        }

        public async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries, int baseDelay)
        {
            return await _retryHelper.ExecuteWithRetriesAsync(funcAsync, operationName, maxRetries, baseDelay);
        }

        public void ExecuteWithRetries(Action action, string operationName, int maxRetries, int baseDelay)
        {
            _retryHelper.ExecuteWithRetries(action, operationName, maxRetries, baseDelay);
        }

        public async Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries, int baseDelay)
        {
            await _retryHelper.ExecuteWithRetriesAsync(actionAsync, operationName, maxRetries, baseDelay);
        }
    }
}
