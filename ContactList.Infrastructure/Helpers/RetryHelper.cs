using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers
{
    public class RetryHelper : IRetryHelper
    {


        public RetryHelper()
        {

        }

        public T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            return RetryHelperStatic.ExecuteWithRetries(func, operationName, maxRetries, baseDelay);
        }

        public void ExecuteWithRetries(Action action, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            RetryHelperStatic.ExecuteWithRetries(action, operationName, maxRetries, baseDelay);
        }

        public async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            return await RetryHelperStatic.ExecuteWithRetriesAsync(funcAsync, operationName, maxRetries, baseDelay);
        }

        public async Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            await RetryHelperStatic.ExecuteWithRetriesAsync(actionAsync, operationName, maxRetries, baseDelay);
        }


    }
}
//Sposób użycia 
//dla Func<T>
//var result = RetryHelper.ExecuteWithRetries(() => SomeMethodFunc(), "SomeMethodFunc");
//var result = await RetryHelper.ExecuteWithRetriesAsync(async () => await SomeAsyncMethod(), "SomeAsyncMethod");
//dla Action
//RetryHelper.ExecuteWithRetriesAsync(() => SomeMethodAction(), "SomeMethodAction");
//await RetryHelper.ExecuteWithRetriesAsync(async () => await SomeMethodAsyncAction(), "SomeMethodAsyncAction");