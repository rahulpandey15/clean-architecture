using System.Threading;
using System.Threading.Tasks;

namespace eCommerce.Domain.Contracts
{
    public interface IBackgroundJobQueue
    {
        void EnqueueOrderProcessing(int orderId);
        Task<int> DequeueOrderAsync(CancellationToken cancellationToken);
    }
}
