using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyBlueWhale.MiniBus.Tests
{
    [TestFixture]
    public sealed class PublishTests
    {
        [SetUp]
        public void SetUp()
        {
            PublishTestState.Reset();
        }

        [Test]
        public async Task Publish_Should_Invoke_All_Event_Handlers()
        {
            var miniBus = CreateMiniBus();

            await miniBus.Publish(new UserCreatedEvent(Guid.NewGuid(),"user@test.com"));

            Assert.Multiple(() =>
            {
                Assert.That(PublishTestState.EmailHandlerCalls, Is.EqualTo(1));
                Assert.That(PublishTestState.AnalyticsHandlerCalls, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task Publish_Should_Complete_When_No_Handlers_Are_Registered()
        {
            var miniBus = CreateMiniBus();

            await miniBus.Publish(new EventWithoutHandlers(Guid.NewGuid()));

            Assert.Pass();
        }

        [Test]
        public async Task Publish_Should_Execute_Handlers_Sequentially()
        {
            var miniBus = CreateMiniBus();

            await miniBus.Publish(new UserCreatedEvent(Guid.NewGuid(), "user@test.com"));

            Assert.That(PublishTestState.ExecutionOrder, Is.EqualTo(new[]
            {
                nameof(SendWelcomeEmailHandler),
                nameof(TrackUserAnalyticsHandler)
            }));
        }

        [Test]
        public void Publish_Should_Throw_When_Event_Is_Null()
        {
            var miniBus = CreateMiniBus();

            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await miniBus.Publish<UserCreatedEvent>(null!));

            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void Publish_Should_Propagate_Exception_When_Handler_Fails()
        {
            var miniBus = CreateMiniBus();

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await miniBus.Publish(new FailingEvent(Guid.NewGuid())));

            Assert.Multiple(() =>
            {
                Assert.That(exception, Is.Not.Null);
                Assert.That(exception!.Message, Is.EqualTo("Event handler failed."));
                Assert.That(PublishTestState.FailingHandlerCalls, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task Publish_Should_Pass_CancellationToken_To_Handlers()
        {
            var miniBus = CreateMiniBus();

            using var cancellationTokenSource = new CancellationTokenSource();

            await miniBus.Publish(new UserCreatedEvent(Guid.NewGuid(), "user@test.com"), cancellationTokenSource.Token);

            Assert.That(PublishTestState.LastCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
        }

        private static IMiniBus CreateMiniBus()
        {
            var services = new ServiceCollection();

            services.AddMiniBus(typeof(SendWelcomeEmailHandler).Assembly);

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IMiniBus>();
        }
    }

    #region Test Messages

    public sealed record UserCreatedEvent(Guid UserId, string Email);

    public sealed record EventWithoutHandlers(Guid Id);

    public sealed record FailingEvent(Guid Id);

    #endregion

    #region Test Handlers

    public sealed class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
    {
        public Task Handle(UserCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            PublishTestState.EmailHandlerCalls++;
            PublishTestState.LastCancellationToken = cancellationToken;
            PublishTestState.ExecutionOrder.Add(nameof(SendWelcomeEmailHandler));

            return Task.CompletedTask;
        }
    }

    public sealed class TrackUserAnalyticsHandler : IEventHandler<UserCreatedEvent>
    {
        public Task Handle(UserCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            PublishTestState.AnalyticsHandlerCalls++;
            PublishTestState.LastCancellationToken = cancellationToken;
            PublishTestState.ExecutionOrder.Add(nameof(TrackUserAnalyticsHandler));

            return Task.CompletedTask;
        }
    }

    public sealed class FailingEventHandler : IEventHandler<FailingEvent>
    {
        public Task Handle(FailingEvent @event, CancellationToken cancellationToken = default)
        {
            PublishTestState.FailingHandlerCalls++;
            PublishTestState.ExecutionOrder.Add(nameof(FailingEventHandler));

            throw new InvalidOperationException("Event handler failed.");
        }
    }

    #endregion

    #region Test State

    internal static class PublishTestState
    {
        public static int EmailHandlerCalls { get; set; }

        public static int AnalyticsHandlerCalls { get; set; }

        public static int FailingHandlerCalls { get; set; }

        public static CancellationToken LastCancellationToken { get; set; }

        public static List<string> ExecutionOrder { get; } = [];

        public static void Reset()
        {
            EmailHandlerCalls = 0;
            AnalyticsHandlerCalls = 0;
            FailingHandlerCalls = 0;
            LastCancellationToken = default;
            ExecutionOrder.Clear();
        }
    }

    #endregion
}