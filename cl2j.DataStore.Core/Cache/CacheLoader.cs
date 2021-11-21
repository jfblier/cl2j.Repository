using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace cl2j.DataStore.Core.Cache
{
    public class CacheLoader
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static Timer timer;
        private readonly string name;
        private readonly TimeSpan refreshInterval;
        private readonly Func<Task> refreshCallback;
        private readonly ILogger logger;
        private bool loaded;

        public CacheLoader(string name, TimeSpan refreshInterval, Func<Task> refreshCallback, ILogger logger)
        {
            this.name = name;
            this.refreshInterval = refreshInterval;
            this.refreshCallback = refreshCallback;
            this.logger = logger;

            logger.LogDebug($"CacheLoader<{name}> Initialized with refresh every {refreshInterval}");

            timer = new Timer(RefreshAsync, null, TimeSpan.Zero, refreshInterval);
        }

        public async Task<bool> WaitAsync()
        {
            var sw = Stopwatch.StartNew();
            while (!loaded && sw.ElapsedMilliseconds <= refreshInterval.TotalMilliseconds)
                Thread.Sleep(100);

            await Task.CompletedTask;
            return loaded;
        }

        private async void RefreshAsync(object state)
        {
            await semaphore.WaitAsync();
            try
            {
                await refreshCallback();
                loaded = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CacheLoader<{name}> : Unexpected error while doing the refresh.");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}