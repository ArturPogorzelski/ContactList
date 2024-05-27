using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Helpers
{
    public interface IRetryHelper
    {
        T ExecuteWithRetries<T>(Func<T> func, string operationName, int maxRetries = 3, int delayBetweenRetries = 1000);
        Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> func, string operationName, int maxRetries = 3, int delayBetweenRetries = 1000);
        void ExecuteWithRetries(Action action, string operationName, int maxRetries = 3, int delayBetweenRetries = 1000);
        Task ExecuteWithRetriesAsync(Func<Task> action, string operationName, int maxRetries = 3, int delayBetweenRetries = 1000);
    }
}
