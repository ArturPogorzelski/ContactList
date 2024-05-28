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
        // Metoda generyczna do wykonania funkcji z automatycznym ponawianiem w przypadku błędów przemijających
        public static T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    // Wykonanie przekazanej funkcji
                    return func();
                }
                catch (Exception ex)
                {
                    // Sprawdzenie, czy błąd jest przemijający i czy nie przekroczono maksymalnej liczby prób
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay); // Czekanie przed kolejną próbą
                                         // delay *= 2; // Opcjonalne: eksponencjalne zwiększanie opóźnienia
                }
            }
        }

        // Metoda wykonująca akcję bez zwracania wyniku z automatycznym ponawianiem
        public static void ExecuteWithRetries(Action action, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    action(); // Wykonanie przekazanej akcji
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
                }
            }
        }

        // Asynchroniczna wersja metody generycznej do wykonania funkcji zwracającej Task<T>
        public static async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> funcAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    return await funcAsync(); // Wykonanie funkcji asynchronicznej
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay); // Czekanie w wersji synchronicznej (Thread.Sleep) zamiast Task.Delay w asynchronicznej
                }
            }
        }

        // Asynchroniczna wersja metody do wykonania akcji asynchronicznej bez zwracania wyniku
        public static async Task ExecuteWithRetriesAsync(Func<Task> actionAsync, string operationName, int maxRetries = 3, int baseDelay = 1000)
        {
            int retries = 0;
            int delay = baseDelay;

            while (true)
            {
                try
                {
                    await actionAsync(); // Wykonanie przekazanej akcji asynchronicznej
                    break;
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || retries >= maxRetries)
                    {
                        throw;
                    }

                    retries++;
                    Thread.Sleep(delay); // Czekanie synchroniczne w kodzie asynchronicznym
                }
            }
        }

        // Metoda do określenia, czy błąd jest przemijający
        private static bool IsTransient(Exception exception)
        {
            // Sprawdzenie, czy wyjątek jest wyjątkiem SQL
            if (exception is SqlException sqlException)
            {
                return TransientErrorNumbers.Contains(sqlException.Number); // Czy kod błędu jest wśród przemijających
            }

            // Obsługa DataException, które może zawierać wyjątek SQL jako pierwotny
            if (exception is DataException dataException && dataException.OriginalException is SqlException dataSqlEx)
            {
                return TransientErrorNumbers.Contains(dataSqlEx.Number);
            }

            return false; // Dla innych typów wyjątków zwraca false
        }

        // Zestawienie kodów błędów SQL uznawanych za przemijające
        private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
    {
        // Przykładowe kody błędów SQL Server
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