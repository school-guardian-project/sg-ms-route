using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps City domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface ICityMapper
{
    CityListResponse ToListResponse(City city);
}

public sealed class CityMapper : ICityMapper
{
    public CityListResponse ToListResponse(City city)
    {
        return new CityListResponse(city.Id, city.Name);
    }
}