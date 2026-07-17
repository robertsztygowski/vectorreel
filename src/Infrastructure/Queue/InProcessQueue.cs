using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Queue;

public delegate Task TaskDispatchDelegate(string queue, string payload, CancellationToken cancellationToken);

public sealed class InProcessQueue(TaskDispatchDelegate dispatch) : ITaskQueue
{
    public Task EnqueueAsync(string queue, string payload, CancellationToken cancellationToken) =>
        dispatch(queue, payload, cancellationToken);
}
