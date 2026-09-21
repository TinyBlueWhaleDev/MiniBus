using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.TestAssets.Duplicates
{
    public sealed record DuplicateRequest(string Value) : IRequest<string>;

    public sealed class FirstDuplicateRequestHandler : IRequestHandler<DuplicateRequest, string>
    {
        public Task<string> Handle(DuplicateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Value);
        }
    }

    public sealed class SecondDuplicateRequestHandler : IRequestHandler<DuplicateRequest, string>
    {
        public Task<string> Handle(DuplicateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Value);
        }
    }
}
