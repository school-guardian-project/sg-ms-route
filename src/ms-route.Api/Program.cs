using Microsoft.AspNetCore.Server.Kestrel.Core;
using ms_route.Api.Infrastructure.DependencyInjection;
using ms_route.Api.Infrastructure.Grpc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Protocols = HttpProtocols.Http1AndHttp2);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddRouteServices(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<RouteGrpcService>();

app.Run();
