//using System.Text;
//var sb = new StringBuilder();
//sb.AppendLine("using TinyBlueWhale.MiniBus;");
//sb.AppendLine("using MediatR;");
//sb.AppendLine("namespace MiniBus.Benchmarks.Handlers;");
//for (int i = 0; i < 2000; i++)
//{
//    sb.AppendLine($$"""
//   public record Request{{i}}(int Id) :
//       MediatR.IRequest<int>,
//       TinyBlueWhale.MiniBus.IRequest<int>;
//   public class Request{{i}}Handler :
//       MediatR.IRequestHandler<Request{{i}}, int>,
//       TinyBlueWhale.MiniBus.IRequestHandler<Request{{i}}, int>
//   {
//       public Task<int> Handle(Request{{i}} request, CancellationToken cancellationToken)
//           => Task.FromResult(request.Id);
//       Task<int> MediatR.IRequestHandler<Request{{i}}, int>.Handle(
//           Request{{i}} request,
//           CancellationToken cancellationToken)
//           => Handle(request, ct);
//   }
//   """);
//}
//sb.AppendLine("""
//public record UserCreatedEvent(int Id) :
//   MediatR.INotification;
//""");
//for (int i = 0; i < 50; i++)
//{
//    sb.AppendLine($$"""
//   public class UserCreatedEvent{{i}}Handler :
//       TinyBlueWhale.MiniBus.IEventHandler<UserCreatedEvent>,
//       MediatR.INotificationHandler<UserCreatedEvent>
//   {
//       public Task Handle(UserCreatedEvent e, CancellationToken cancellationToken)
//           => Task.CompletedTask;
//       Task MediatR.INotificationHandler<UserCreatedEvent>.Handle(
//           UserCreatedEvent notification,
//           CancellationToken cancellationToken)
//           => Handle(notification, ct);
//   }
//   """);
//}
//var basePath = AppContext.BaseDirectory;
//var projectPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", ".."));
//var handlersDir = Path.Combine(projectPath, "Handlers");
//Directory.CreateDirectory(handlersDir);
//var filePath = Path.Combine(handlersDir, "GeneratedHandlers.cs");
//File.WriteAllText(filePath, sb.ToString());
//Console.WriteLine($"Generado en: {filePath}");


using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);