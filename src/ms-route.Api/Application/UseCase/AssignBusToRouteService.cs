using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class AssignBusToRouteService : IAssignBusToRouteUseCase
{
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;

    public AssignBusToRouteService(
        IRouteBusAssignmentRepository assignmentRepository,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
    }

    public async Task<BusAssignmentDto> ExecuteAsync(Guid routeId, Guid busId, CancellationToken ct = default)
    {
        var routeHasBus = await _assignmentRepository.RouteHasBusAsync(routeId, ct);
        if (routeHasBus)
            throw new InvalidOperationException("The route already has a bus assigned");

        var assignment = new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            BusId = busId,
            RouteId = routeId,
            Status = Status.Active
        };

        var saved = await _assignmentRepository.SaveAsync(assignment, ct);

        return _mapper.Map<BusAssignmentDto>(saved);
    }
}
