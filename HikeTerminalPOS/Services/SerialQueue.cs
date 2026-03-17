using System;
using HikePOS.Helpers;
namespace HikePOS.Services
{
    public sealed class SerialQueue
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task Enqueue(Func<Task> work)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await work().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("SerialQueue error: " + ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

