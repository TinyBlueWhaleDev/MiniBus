using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.TestAssets.Primary
{
    public sealed record PrimaryRequest(string Value) : IRequest<string>;

    public sealed class PrimaryRequestHandler : IRequestHandler<PrimaryRequest, string>
    {
        public Task<string> Handle(PrimaryRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Primary:{request.Value}");
        }
    }

    public sealed record SharedEvent(string Value);

    public static class PrimaryEventState
    {
        public static int Calls { get; set; }

        public static void Reset()
        {
            Calls = 0;
        }
    }

    public sealed class PrimaryEventHandler : IEventHandler<SharedEvent>
    {
        public Task Handle(SharedEvent @event, CancellationToken cancellationToken)
        {
            PrimaryEventState.Calls++;

            return Task.CompletedTask;
        }
    }
}
