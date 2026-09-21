using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.TestAssets.Secondary
{
    public sealed record SecondaryRequest(string Value) : IRequest<string>;

    public sealed class SecondaryRequestHandler : IRequestHandler<SecondaryRequest, string>
    {
        public Task<string> Handle(SecondaryRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Secondary:{request.Value}");
        }
    }
}
