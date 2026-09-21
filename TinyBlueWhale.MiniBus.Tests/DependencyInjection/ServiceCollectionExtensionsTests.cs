using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TinyBlueWhale.MiniBus.Abstractions;
using TinyBlueWhale.MiniBus.DependencyInjection;
using TinyBlueWhale.MiniBus.TestAssets.Duplicates;
using TinyBlueWhale.MiniBus.TestAssets.Primary;
using TinyBlueWhale.MiniBus.TestAssets.Secondary;

namespace TinyBlueWhale.MiniBus.Tests.DependencyInjection
{
    [TestFixture]
    public sealed class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddMiniBus_Should_Throw_When_Services_Is_Null()
        {
            IServiceCollection services = null!;

            var exception = Assert.Throws<ArgumentNullException>(() => services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly));

            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void AddMiniBus_Should_Throw_When_Assemblies_Is_Null()
        {
            var services = new ServiceCollection();

            Assembly[] assemblies = null!;

            var exception = Assert.Throws<ArgumentNullException>(() => services.AddMiniBus(assemblies));

            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void AddMiniBus_Should_Throw_When_Multiple_Request_Handlers_Are_Registered()
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<InvalidOperationException>(() => services.AddMiniBus(typeof(DuplicateRequest).Assembly));

            Assert.That(exception!.Message, Is.EqualTo($"Multiple request handlers registered for '{typeof(DuplicateRequest).FullName}'."));
        }

        [Test]
        public async Task AddMiniBus_Should_Discover_Handlers_From_Multiple_Assemblies()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly, 
                typeof(SecondaryRequestHandler).Assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var miniBus = serviceProvider.GetRequiredService<IMiniBus>();

            var primaryResult = await miniBus.Send(new PrimaryRequest("one"));

            var secondaryResult = await miniBus.Send(new SecondaryRequest("two"));

            Assert.Multiple(() =>
            {
                Assert.That(primaryResult, Is.EqualTo("Primary:one"));

                Assert.That(secondaryResult, Is.EqualTo("Secondary:two"));
            });
        }

        [Test]
        public async Task AddMiniBus_Should_Ignore_Duplicate_Assemblies()
        {
            var services = new ServiceCollection();

            var assembly = typeof(PrimaryRequestHandler).Assembly;

            services.AddMiniBus(assembly, assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var miniBus = serviceProvider.GetRequiredService<IMiniBus>();

            var result = await miniBus.Send(new PrimaryRequest("value"));

            Assert.That(result, Is.EqualTo("Primary:value"));
        }

        [Test]
        public void AddMiniBus_Should_Register_IMiniBus()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var miniBus = serviceProvider.GetService<IMiniBus>();

            Assert.That(miniBus, Is.Not.Null);
        }

        [Test]
        public void AddMiniBus_Should_Return_Same_ServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);

            Assert.That(result, Is.SameAs(services));
        }

        [Test]
        public void AddMiniBus_Should_Register_Handler()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);

            using var serviceProvider =
                services.BuildServiceProvider();

            var handler = serviceProvider.GetService<PrimaryRequestHandler>();

            Assert.That(handler, Is.Not.Null);
        }

        [Test]
        public void AddMiniBus_Should_Register_Handler_As_Transient()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var first = serviceProvider.GetRequiredService<PrimaryRequestHandler>();

            var second = serviceProvider.GetRequiredService<PrimaryRequestHandler>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void AddMiniBus_Should_Register_IMiniBus_As_Scoped()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);

            using var serviceProvider = services.BuildServiceProvider();

            IMiniBus first;
            IMiniBus second;

            using (var scope = serviceProvider.CreateScope())
            {
                first = scope.ServiceProvider.GetRequiredService<IMiniBus>();

                var sameScope = scope.ServiceProvider.GetRequiredService<IMiniBus>();

                Assert.That(sameScope, Is.SameAs(first));
            }

            using (var scope = serviceProvider.CreateScope())
            {
                second = scope.ServiceProvider.GetRequiredService<IMiniBus>();
            }

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public async Task AddMiniBus_Should_Discover_Handlers_Across_Multiple_Registrations()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(PrimaryRequestHandler).Assembly);
            services.AddMiniBus(typeof(SecondaryRequestHandler).Assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var miniBus = serviceProvider.GetRequiredService<IMiniBus>();

            var primaryResult = await miniBus.Send(new PrimaryRequest("one"));
            var secondaryResult = await miniBus.Send(new SecondaryRequest("two"));

            Assert.Multiple(() =>
            {
                Assert.That(primaryResult, Is.EqualTo("Primary:one"));
                Assert.That(secondaryResult, Is.EqualTo("Secondary:two"));
            });
        }

        [Test]
        public async Task AddMiniBus_Should_Ignore_Assembly_Registered_Multiple_Times()
        {
            var services = new ServiceCollection();

            var assembly = typeof(PrimaryRequestHandler).Assembly;

            services.AddMiniBus(assembly);
            services.AddMiniBus(assembly);

            using var serviceProvider = services.BuildServiceProvider();

            var miniBus = serviceProvider.GetRequiredService<IMiniBus>();

            var result = await miniBus.Send(new PrimaryRequest("value"));

            Assert.That(result, Is.EqualTo("Primary:value"));
        }

        [Test]
        public void AddMiniBus_Should_Throw_When_No_Assemblies_Are_Provided()
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<ArgumentException>(() => services.AddMiniBus());

            Assert.That(exception!.ParamName, Is.EqualTo("assemblies"));
        }

        [Test]
        public void AddMiniBus_Should_Throw_When_Assemblies_Contain_Null()
        {
            var services = new ServiceCollection();

            var assemblies = new[]
            {
                typeof(PrimaryRequestHandler).Assembly,
                null!
            };

            var exception = Assert.Throws<ArgumentException>(() => services.AddMiniBus(assemblies));

            Assert.That(exception!.ParamName, Is.EqualTo("assemblies"));
        }
    }
}
