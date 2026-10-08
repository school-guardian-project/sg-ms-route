using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ms_route.Api.Infrastructure.DependencyInjection;
using ms_route.Api.Infrastructure.Grpc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddRouteServices(builder.Configuration);
builder.Services.AddGrpc();

// JWT emitido por ms-iam (HS256, sin issuer/audience). Solo autenticación:
// se usa para filtrar por tenant; no hay autorización ([Authorize]).
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? "your-256-bit-base64-encoded-secret-min-32-chars";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseAuthentication();
app.MapControllers();
app.MapGrpcService<RouteGrpcService>();

app.Run();
