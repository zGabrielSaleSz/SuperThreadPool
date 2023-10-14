using System.Net.NetworkInformation;

namespace SuperThreadPool.Tests.dotnet
{
    public class AsyncAwaitIsNotParallel
    {
        [Fact]
        public void Should_SwitchThreads_When_AsyncAwaitIsUsed()
        {
            // Arrange
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            ExecuteDelayedOperationAndGetResultThread(threadId,
                TimeSpan.FromSeconds(1))
                .ContinueWith((rTask) =>
                {
                    int threadBeforeSynchronousAwaiter = Thread.CurrentThread.ManagedThreadId;
                    int threadIdAfterAwait = rTask.GetAwaiter().GetResult();
                    int currentThreadId = Thread.CurrentThread.ManagedThreadId;

                    // Are equal when using synchronousAwaiter
                    Assert.Equal(threadBeforeSynchronousAwaiter, currentThreadId);

                    // Are different from same context
                    Assert.NotEqual(threadId, currentThreadId);
                    Assert.NotEqual(threadIdAfterAwait, currentThreadId);

                    autoResetEvent.Set();
                }
            );

            autoResetEvent.WaitOne();
        }

        private async Task<int> ExecuteDelayedOperationAndGetResultThread(int inokedThread, TimeSpan timeSpan)
        {
            Assert.Equal(inokedThread, Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(timeSpan);

            // code below returns from different thread
            Assert.NotEqual(inokedThread, Thread.CurrentThread.ManagedThreadId);
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}