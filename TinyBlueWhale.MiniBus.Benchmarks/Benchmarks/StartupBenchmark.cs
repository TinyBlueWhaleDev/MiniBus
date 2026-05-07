using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MiniBus.Benchmarks.Handlers;
using TinyBlueWhale.MiniBus;
namespace MiniBus.Benchmarks.Benchmarks;
[MemoryDiagnoser]
public class StartupBenchmark
{
    [Benchmark(Baseline = true)]
    public void MiniBus_Add()
    {
        var services = new ServiceCollection();
        services.AddMiniBus(typeof(Request0).Assembly);
        services.BuildServiceProvider();
    }
    [Benchmark]
    public void MediatR_Add()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Request0>());
        services.BuildServiceProvider();
    }
}