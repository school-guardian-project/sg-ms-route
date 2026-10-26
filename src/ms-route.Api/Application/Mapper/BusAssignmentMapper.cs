using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps RouteBusAssignment domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IBusAssignmentMapper
{
    BusAssignmentResponse ToResponse(RouteBusAssignment assignment);
}

public sealed class BusAssignmentMapper : IBusAssignmentMapper
{
    public BusAssignmentResponse ToResponse(RouteBusAssignment assignment)
    {
        return new BusAssignmentResponse(
            assignment.Id,
            assignment.RouteId,
            assignment.BusId,
            assignment.Status.ToString());
    }
}