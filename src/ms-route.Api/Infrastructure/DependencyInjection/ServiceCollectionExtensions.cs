using Microsoft.EntityFrameworkCore;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Repository;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRouteServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RouteContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddScoped<IRouteRepository, RouteRepositoryImpl>();
        services.AddScoped<IStopRepository, StopRepositoryImpl>();

        services.AddScoped<ICreateRouteUseCase, CreateRouteService>();
        services.AddScoped<IGetRouteUseCase, GetRouteService>();
        services.AddScoped<IListRouteUseCase, ListRouteService>();
        services.AddScoped<IUpdateRouteUseCase, UpdateRouteService>();
        services.AddScoped<IDeleteRouteUseCase, DeleteRouteService>();

        services.AddScoped<ICreateStopUseCase, CreateStopService>();
        services.AddScoped<IGetStopUseCase, GetStopService>();
        services.AddScoped<IListStopUseCase, ListStopService>();
        services.AddScoped<IUpdateStopUseCase, UpdateStopService>();
        services.AddScoped<IDeleteStopUseCase, DeleteStopService>();

        return services;
    }
}
