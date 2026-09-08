namespace AudioProcessor.Tests;

public class ProcessingConcurrencyLimitTests
{
    [Fact]
    public async Task Semaphore_NeverAllowsMoreConcurrentJobsThanItsLimit()
    {
        const int maxConcurrentJobs = 3;
        const int totalJobs = maxConcurrentJobs * 2;

        using var semaphore = new SemaphoreSlim(maxConcurrentJobs, maxConcurrentJobs);
        var currentCount = 0;
        var maxObservedCount = 0;
        var lockObj = new object();

        async Task RunJobAsync()
        {
            await semaphore.WaitAsync();
            try
            {
                lock (lockObj)
                {
                    currentCount++;
                    maxObservedCount = Math.Max(maxObservedCount, currentCount);
                }

                await Task.Delay(50);
            }
            finally
            {
                lock (lockObj)
                {
                    currentCount--;
                }

                semaphore.Release();
            }
        }

        var jobs = Enumerable.Range(0, totalJobs).Select(_ => RunJobAsync());
        await Task.WhenAll(jobs);

        Assert.True(maxObservedCount <= maxConcurrentJobs);
        Assert.Equal(0, currentCount);
    }
}
