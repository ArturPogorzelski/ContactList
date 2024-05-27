using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers
{
    public static class RetryHelperStatic
    {

        public static T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay);
                    // delay *= 2; // Opcjonalne eksponencjalne zwiększanie opóźnienia
                }
            }
        }

        public static void ExecuteWithRetries(Action action, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay);
                    // delay *= 2; // Opcjonalne eksponencjalne zwiększanie opóźnienia
                }
            }
        }
        public static async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    return await funcAsync();
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay);
                    // delay *= 2; // Opcjonalne eksponencjalne zwiększanie opóźnienia
                }
            }
        }
        public static async Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    await actionAsync();
                    break;
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay);
                    // delay *= 2; // Opcjonalne eksponencjalne zwiększanie opóźnienia
                }
            }
        }
        private static bool IsTransient(Exception exception)
        {
            // Jeśli wyjątek jest wyjątkiem SQL, dokonujemy sprawdzenia na podstawie kodów błędów
            if (exception is SqlException sqlException)
            {
                return TransientErrorNumbers.Contains(sqlException.Number);
            }

            // Dodanie obsługi DataException
            if (exception is DataException dataException && dataException.OriginalException is SqlException dataSqlEx)
            {
                return TransientErrorNumbers.Contains(dataSqlEx.Number);
            }

            // Dla wszystkich innych wyjątków zwracamy false
            return false;
        }
        private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
            {
                // Lista kodów błędów SQL Server uznawanych za przemijające, w tym 1205
                -2, 20, 64, 233, 10053, 10054, 10060, 10928, 10929, 40143, 40197, 40501, 40613, 41301, 41302, 41305, 41325, 41839, 49918, 49919, 49920, 1205
            };
    }
}
//Sposób użycia 
//dla Func<T>
//var result = RetryHelper.ExecuteWithRetries(() => SomeMethodFunc(), "SomeMethodFunc");
//var result = await RetryHelper.ExecuteWithRetriesAsync(async () => await SomeAsyncMethod(), "SomeAsyncMethod");
//dla Action
//RetryHelper.ExecuteWithRetriesAsync(() => SomeMethodAction(), "SomeMethodAction");
//await RetryHelper.ExecuteWithRetriesAsync(async () => await SomeMethodAsyncAction(), "SomeMethodAsyncAction");