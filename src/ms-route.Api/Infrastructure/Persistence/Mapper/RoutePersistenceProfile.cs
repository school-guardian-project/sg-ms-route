using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Infrastructure.Persistence.Mapper;

public class RoutePersistenceProfile : Profile
{
    public RoutePersistenceProfile()
    {
        CreateMap<RouteModel, RouteEntity>();
        CreateMap<RouteEntity, RouteModel>();
    }
}
