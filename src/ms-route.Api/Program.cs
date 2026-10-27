using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ms_route.Api.Domain.Exceptions;
using ms_route.Api.Infrastructure.Auth;
using ms_route.Api.Infrastructure.DependencyInjection;
using ms_route.Api.Infrastructure.Grpc;
using ms_route.Api.Infrastructure.Grpc.Server;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Techo global (no depende de IP ni de usuario): acota el trabajo simultaneo
    // y la rafaga para que un flood desde una sola IP, o con IP falsificada via
    // cabeceras, nunca sature el backend. La concurrencia va primero para que un
    // rechazo de la ventana no consuma cupo ya adquirido.
    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetConcurrencyLimiter("global", _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 40, // ~ Max Pool Size (conexiones simultaneas a BD)
                QueueLimit = 0
            })),
        PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            })));

    // Escrituras (alta/edicion/borrado): 30/min por IP para frenar abuso.
    options.AddPolicy("writes", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

builder.Services.AddRouteServices(builder.Configuration);
builder.Services.AddGrpc();

// El servicio valida por su cuenta el access token RS256 de ms-iam (JWKS):
// la identidad sale del token, no de las cabeceras X-User-* del gateway.
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();

// Errores de dominio como ProblemDetails (400/404/409) en vez de un 500 con
// traza: el frontend muestra `detail` dentro del modal que hizo la accion.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (AccessDeniedException ex) when (!context.Response.HasStarted)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(context);
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException && !context.Response.HasStarted)
    {
        var status = ex is ArgumentException
            ? StatusCodes.Status400BadRequest
            : ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status409Conflict;
        context.Response.StatusCode = status;
        await Results.Problem(detail: ex.Message, statusCode: status).ExecuteAsync(context);
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
// Todo endpoint REST exige un access token valido. El gRPC interno
// (RouteGrpcService, puerto 5001, consumido por ms-notification) no pasa por aqui.
app.MapControllers().RequireAuthorization();
app.MapGrpcService<RouteGrpcService>();

app.Run();
