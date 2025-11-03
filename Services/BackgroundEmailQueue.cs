using System.Collections.Concurrent;
using WebApiVotacionElectronica.Models.Tools;

namespace WebApiVotacionElectronica.Services
{
    public interface IBackgroundEmailQueue
    {
        void Enqueue(EmailWorkItem workItem);
        Task<EmailWorkItem> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private ConcurrentQueue<EmailWorkItem> _workItems = new();
        private SemaphoreSlim _signal = new(0);

        public void Enqueue(EmailWorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<EmailWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem;
        }
    }
}
