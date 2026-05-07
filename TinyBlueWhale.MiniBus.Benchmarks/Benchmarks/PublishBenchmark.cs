using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MiniBus.Benchmarks.Handlers;
using TinyBlueWhale.MiniBus;
namespace MiniBus.Benchmarks.Benchmarks;
[MemoryDiagnoser]
public class PublishBenchmark
{
    private IMiniBus _miniBus = null!;
    private MediatR.IMediator _mediator = null!;
    private readonly UserCreatedEvent _evt = new(42);
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMiniBus(typeof(UserCreatedEvent).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UserCreatedEvent>());
        var sp = services.BuildServiceProvider();
        _miniBus = sp.GetRequiredService<IMiniBus>();
        _mediator = sp.GetRequiredService<MediatR.IMediator>();
    }
    [Benchmark(Baseline = true)]
    public async Task MiniBus_Publish_50Handlers() => await _miniBus.Publish(_evt);
    [Benchmark]
    public async Task MediatR_Publish_50Handlers() => await _mediator.Publish(_evt);
}