using TinyBlueWhale.MiniBus;
using TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMiniBus(typeof(CreateOrderHandler).Assembly);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapCreateOrderEndpoint();


app.Run();