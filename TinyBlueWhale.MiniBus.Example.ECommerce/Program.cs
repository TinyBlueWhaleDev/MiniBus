using TinyBlueWhale.MiniBus.DependencyInjection;
using TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders;
using TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder;
using TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.GetOrder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OrderStore>();
builder.Services.AddMiniBus(typeof(CreateOrderHandler).Assembly);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapCreateOrderEndpoint();
app.MapGetOrderEndpoint();

app.Run();