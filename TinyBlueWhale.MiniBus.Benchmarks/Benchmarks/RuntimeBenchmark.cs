using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MiniBus.Benchmarks.Handlers;
using TinyBlueWhale.MiniBus;
namespace MiniBus.Benchmarks.Benchmarks;
[MemoryDiagnoser]
public class RuntimeBenchmark
{
    private IMiniBus _miniBus = null!;
    private MediatR.IMediator _mediator = null!;
    private readonly Request0 _req = new(42);
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMiniBus(typeof(Request0).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Request0>());
        var sp = services.BuildServiceProvider();
        _miniBus = sp.GetRequiredService<IMiniBus>();
        _mediator = sp.GetRequiredService<MediatR.IMediator>();
    }
    [Benchmark(Baseline = true)]
    public async Task<int> MiniBus_Send() => await _miniBus.Send(_req);
    [Benchmark]
    public async Task<int> MediatR_Send() => await _mediator.Send(_req);
}