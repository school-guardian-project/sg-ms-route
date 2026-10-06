using AutoMapper;
using ms_route.Api.Domain.Model;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Persistence.Mapper;

public class StopPersistenceProfile : Profile
{
    public StopPersistenceProfile()
    {
        CreateMap<Stop, StopEntity>();
        CreateMap<StopEntity, Stop>();
    }
}
