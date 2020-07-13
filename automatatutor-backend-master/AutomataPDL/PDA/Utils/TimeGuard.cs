using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Utils
{
    public static class TimeGuard<T>
    {
        public static T Run(Func<CancellationToken, T> action, int timeOutMilliseconds)
        {
            var c = new CancellationTokenSource();
            var token = c.Token;

            try
            {
                c.CancelAfter(timeOutMilliseconds);
                var task = Task.Run(() => action(token));
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OperationCanceledException)
                {
                    throw new TimeoutException();
                }
                else
                {
                    throw ex.InnerException;
                }
            }
        }
    }
}
