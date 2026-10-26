using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps RouteExecution domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface ITripMapper
{
    TripResponse ToResponse(RouteExecution execution);
}

public sealed class TripMapper : ITripMapper
{
    public TripResponse ToResponse(RouteExecution execution)
    {
        return new TripResponse(
            execution.Id,
            execution.RouteId,
            execution.BusId,
            execution.DriverId,
            execution.StartDateTime,
            execution.EndDateTime,
            execution.Status.ToString(),
            execution.Direction);
    }
}