using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace cl2j.DataStore.Core.Cache
{
    public class CacheLoader
    {
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly string name;
        private readonly TimeSpan refreshInterval;
        private readonly Func<Task> refreshCallback;
        private readonly ILogger logger;
        private bool loaded;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly Timer timer;
#pragma warning restore IDE0052 // Remove unread private members

        public CacheLoader(string name, TimeSpan refreshInterval, Func<Task> refreshCallback, ILogger logger)
        {
            this.name = name;
            this.refreshInterval = refreshInterval;
            this.refreshCallback = refreshCallback;
            this.logger = logger;

#pragma warning disable CA2254 // Template should be a static expression
            logger.LogDebug($"CacheLoader<{name}> Initialized with refresh every {refreshInterval}");
#pragma warning restore CA2254 // Template should be a static expression

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

        private async void RefreshAsync(object? state)
        {
            await semaphore.WaitAsync();
            try
            {
                await refreshCallback();
                loaded = true;
            }
            catch (Exception ex)
            {
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogError(ex, $"CacheLoader<{name}> : Unexpected error while doing the refresh.");
#pragma warning restore CA2254 // Template should be a static expression
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}