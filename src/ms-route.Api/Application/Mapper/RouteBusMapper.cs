using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps Bus domain entities (from ms-fleet) to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IRouteBusMapper
{
    RouteBusResponse ToResponse(
        Guid busId,
        string plate,
        int capacity,
        Guid? driverProfileId,
        string driverName);
}

public sealed class RouteBusMapper : IRouteBusMapper
{
    public RouteBusResponse ToResponse(
        Guid busId,
        string plate,
        int capacity,
        Guid? driverProfileId,
        string driverName)
    {
        return new RouteBusResponse(busId, plate, capacity, driverProfileId, driverName);
    }
}