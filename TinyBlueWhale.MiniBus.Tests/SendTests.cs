using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace TinyBlueWhale.MiniBus.Tests;

[TestFixture]
public sealed class SendTests
{
    [Test]
    public async Task Send_Should_Dispatch_Request_And_Return_Response()
    {
        var miniBus = CreateMiniBus();

        var result = await miniBus.Send(new CreateUserCommand("user@test.com"));

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Send_Should_Dispatch_Query_And_Return_Dto()
    {
        var miniBus = CreateMiniBus();

        var userId = Guid.NewGuid();

        var result = await miniBus.Send(new GetUserQuery(userId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.Email, Is.EqualTo("user@test.com"));
        });
    }

    [Test]
    public void Send_Should_Throw_When_Handler_Is_Missing()
    {
        var miniBus = CreateMiniBus();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await miniBus.Send(new MissingHandlerQuery(Guid.NewGuid())));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Does.Contain("No handler registered"));
        });
    }

    [Test]
    public void Send_Should_Throw_When_Request_Is_Null()
    {
        var miniBus = CreateMiniBus();

        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await miniBus.Send<Guid>(null!));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public async Task Send_Should_Pass_CancellationToken_To_Handler()
    {
        CancellationTokenTracker.Reset();

        var miniBus = CreateMiniBus();

        using var cancellationTokenSource = new CancellationTokenSource();

        var result = await miniBus.Send(
            new CancellationCommand("ok"),
            cancellationTokenSource.Token);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("ok"));
            Assert.That(
                CancellationTokenTracker.LastCancellationToken,
                Is.EqualTo(cancellationTokenSource.Token));
        });
    }

    private static IMiniBus CreateMiniBus()
    {
        var services = new ServiceCollection();

        services.AddMiniBus(typeof(CreateUserHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IMiniBus>();
    }
}

#region Test Messages

public sealed record CreateUserCommand(string Email) : IRequest<Guid>;

public sealed record GetUserQuery(Guid UserId) : IRequest<UserDto>;

public sealed record MissingHandlerQuery(Guid Id) : IRequest<string>;

public sealed record CancellationCommand(string Value) : IRequest<string>;

public sealed record UserDto(Guid Id, string Email);

#endregion

#region Test Handlers

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new UserDto(
            request.UserId,
            "user@test.com"));
    }
}

public sealed class CancellationCommandHandler : IRequestHandler<CancellationCommand, string>
{
    public Task<string> Handle(CancellationCommand request, CancellationToken cancellationToken = default)
    {
        CancellationTokenTracker.LastCancellationToken = cancellationToken;

        return Task.FromResult(request.Value);
    }
}

#endregion

#region Test State

internal static class CancellationTokenTracker
{
    public static CancellationToken LastCancellationToken { get; set; }

    public static void Reset()
    {
        LastCancellationToken = default;
    }
}

#endregion