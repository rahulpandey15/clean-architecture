using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using eCommerce.Domain.Contracts;

namespace eCommerce.Infrastructure.Background
{
    public class BackgroundJobQueue : IBackgroundJobQueue
    {
        private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();

        public void EnqueueOrderProcessing(int orderId)
        {
            // Fire-and-forget enqueue
            _ = _queue.Writer.WriteAsync(orderId);
        }

        public async Task<int> DequeueOrderAsync(CancellationToken cancellationToken)
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken);
            return item;
        }
    }
}
