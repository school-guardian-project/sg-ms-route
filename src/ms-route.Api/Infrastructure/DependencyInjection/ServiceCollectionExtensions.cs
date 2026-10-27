using Microsoft.EntityFrameworkCore;
using ms_route.Api.Application.Mapper;
using ms_route.Api.Application.Search.Route;
using ms_route.Api.Application.Search.Stop;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Port.Out;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;
using ms_route.Api.Infrastructure.Auth;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Mapper;
using ms_route.Api.Infrastructure.Grpc;
using ms_route.Api.Infrastructure.Repository;
using ms_route.Api.Infrastructure.Routing;

using RouteNameSearchStrategy = ms_route.Api.Application.Search.Route.NameSearchStrategy;
using StopNameSearchStrategy = ms_route.Api.Application.Search.Stop.NameSearchStrategy;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRouteServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<RouteProfile>();
            cfg.AddProfile<StopProfile>();
            cfg.AddProfile<RoutePersistenceProfile>();
            cfg.AddProfile<StopPersistenceProfile>();
        });

        services.AddDbContextPool<RouteContext>(
            (sp, options) => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)),
            poolSize: 64);

        services.AddScoped<IRouteRepository, RouteRepositoryImpl>();
        services.AddScoped<IStopRepository, StopRepositoryImpl>();
        services.AddScoped<IRouteExecutionRepository, RouteExecutionRepositoryImpl>();
        services.AddScoped<IRouteStudentAssignmentRepository, RouteStudentAssignmentRepositoryImpl>();
        services.AddScoped<IRouteStopRepository, RouteStopRepositoryImpl>();
        services.AddScoped<IRouteBusAssignmentRepository, RouteBusAssignmentRepositoryImpl>();
        services.AddScoped<IRouteScheduleRepository, RouteScheduleRepositoryImpl>();
        services.AddScoped<ICityRepository, CityRepositoryImpl>();
        services.AddScoped<ISchoolCampusRepository, SchoolCampusRepositoryImpl>();

        // Trip lifecycle services (Domain + Infrastructure)
        services.AddScoped<ITripBoardingQuery, TripBoardingQueryRepository>();
        services.AddScoped<ITripLifecycleService, TripLifecycleService>();
        services.AddScoped<IGetTripStatusMapper, GetTripStatusMapper>();

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, JwtTenantProvider>();

        // Los buses (placa/conductor) los expone ms-fleet a través de gRPC.
        // Singleton porque cada cliente GrpcChannel abre conexiones que conviene
        // reutilizar entre peticiones.
        services.AddSingleton<IFleetService, FleetGrpcClient>();

        services.AddSingleton(TimeProvider.System);

        // TimeZone para Colombia (UTC-5, sin horario de verano)
        services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("America/Bogota"));

        services.AddScoped<ICreateRouteUseCase, CreateRouteService>();
        services.AddScoped<IGetRouteUseCase, GetRouteService>();
        services.AddScoped<IGetRouteByExecutionIdUseCase, GetRouteByExecutionIdService>();
        services.AddScoped<IListRouteUseCase, ListRouteService>();
        services.AddScoped<IUpdateRouteUseCase, UpdateRouteService>();
        services.AddScoped<IDeleteRouteUseCase, DeleteRouteService>();

        services.AddScoped<ICreateStopUseCase, CreateStopService>();
        services.AddScoped<IGetStopUseCase, GetStopService>();
        services.AddScoped<IListStopUseCase, ListStopService>();
        services.AddScoped<IUpdateStopUseCase, UpdateStopService>();
        services.AddScoped<IDeleteStopUseCase, DeleteStopService>();

        services.AddScoped<IListCityUseCase, ListCityService>();
        services.AddScoped<IAttachStopToRouteUseCase, AttachStopToRouteService>();

        services.AddScoped<IRouteSearchStrategy, ScheduleSearchStrategy>();
        services.AddScoped<IRouteSearchStrategy, RouteNameSearchStrategy>();
        services.AddScoped<IStopSearchStrategy, StopNameSearchStrategy>();
        services.AddScoped<ISearchRouteUseCase, SearchRouteService>();
        services.AddScoped<ISearchStopUseCase, SearchStopService>();

        services.AddScoped<IGetCurrentRouteUseCase, GetCurrentRouteService>();
        services.AddScoped<IGetDriverRouteTodayUseCase, GetDriverRouteTodayService>();
        services.AddScoped<IGetRouteSuggestionUseCase, GetRouteSuggestionUseCase>();
        services.AddScoped<IStartTripUseCase, StartTripService>();
        services.AddScoped<IEndTripUseCase, EndTripService>();
        services.AddScoped<IGetCurrentTripUseCase, GetCurrentTripService>();
        services.AddScoped<IGetTripStatusUseCase, GetTripStatusService>();
        services.AddScoped<IGetStudentStopOnRouteUseCase, GetStudentStopOnRouteService>();
        services.AddScoped<IGetStudentRouteUseCase, GetStudentRouteService>();
        services.AddScoped<IRouteStopFilter, RouteStopFilter>();
        services.AddScoped<IRouteDetailPopulator, RouteDetailPopulator>();
        services.AddScoped<IAssignStudentToRouteUseCase, AssignStudentToRouteService>();
        services.AddScoped<IAssignBusToRouteUseCase, AssignBusToRouteService>();
        services.AddScoped<IUnassignBusFromRouteUseCase, UnassignBusFromRouteService>();
        services.AddScoped<ICreateRouteScheduleUseCase, CreateRouteScheduleService>();
        services.AddScoped<IListRouteSchedulesUseCase, ListRouteSchedulesService>();

        services.AddSingleton<IMapRoutingService, OsrmMapRoutingService>();

        return services;
    }
}
