using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetRouteService : IGetRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;
    private readonly IMapper _mapper;
    private readonly IFleetService? _fleetService;

    public GetRouteService(
        IRouteRepository repository,
        IRouteBusAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IMapper mapper,
        IFleetService? fleetService = null)
    {
        _fleetService = fleetService;
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _mapper = mapper;
    }

    public async Task<RouteResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var route = await _repository.GetByIdAsync(id, ct);
        if (route is null)
            throw new InvalidOperationException($"Route not found: {id}");

        var dto = _mapper.Map<RouteResponseDto>(route);

        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(id, ct);
        dto.BusId = assignment?.BusId;
        if (assignment is not null && _fleetService is not null)
        {
            try
            {
                var bus = await _fleetService.GetBusDetailsAsync(assignment.BusId, ct);
                if (bus is not null)
                {
                    dto.Bus = new RouteBusDto
                    {
                        Id = bus.Id,
                        Plate = bus.Plate,
                        Capacity = bus.Capacity,
                        DriverProfileId = bus.DriverProfileId,
                        DriverName = bus.DriverName
                    };
                }
            }
            catch (Exception)
            {
                // El detalle de la ruta no debe fallar si ms-fleet no responde.
            }
        }

        var routeStops = await _routeStopRepository.GetByRouteIdAsync(id, ct);
        var stops = await _stopRepository.GetByIdsAsync(routeStops.Select(rs => rs.StopId), ct);
        var stopById = stops.ToDictionary(s => s.Id);

        dto.Stops = routeStops
            .Where(rs => rs.Status == Status.Active && stopById.ContainsKey(rs.StopId))
            .Select(rs => new StopDetailDto
            {
                Id = rs.StopId,
                Name = stopById[rs.StopId].Name,
                Address = stopById[rs.StopId].Address,
                Latitude = stopById[rs.StopId].Latitude,
                Longitude = stopById[rs.StopId].Longitude,
                OrderSequence = rs.OrderSequence
            })
            .ToList();

        return dto;
    }
}
