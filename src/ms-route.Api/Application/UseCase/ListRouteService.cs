using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListRouteService : IListRouteUseCase
{
    private const int RoleAdmin = 1;

    private readonly IRouteRepository _repository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly ISchoolCampusRepository _schoolCampusRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;

    public ListRouteService(
        IRouteRepository repository,
        IRouteStopRepository routeStopRepository,
        ISchoolCampusRepository schoolCampusRepository,
        ITenantProvider tenantProvider,
        IMapper mapper)
    {
        _repository = repository;
        _routeStopRepository = routeStopRepository;
        _schoolCampusRepository = schoolCampusRepository;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RouteListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var campusIds = await ResolveCampusFilterAsync(ct);

        var routes = await _repository.GetAllAsync(campusIds, ct);

        var result = _mapper.Map<List<RouteListDto>>(routes);

        var stopsCount = await _routeStopRepository.CountByRouteIdsAsync(result.Select(r => r.Id), ct);
        foreach (var route in result)
        {
            stopsCount.TryGetValue(route.Id, out var count);
            route.StopsCount = count;
        }

        return result;
    }

    /// <summary>
    /// Resuelve las sedes visibles según el tenant del JWT:
    /// Admin (1) con schoolId => todas las sedes del colegio;
    /// Student/Driver/Parent (2-4) con campusId => solo su sede.
    /// null => sin filtro (sin token, token inválido o SuperAdmin).
    /// </summary>
    private async Task<IReadOnlyCollection<Guid>?> ResolveCampusFilterAsync(CancellationToken ct)
    {
        if (!_tenantProvider.ShouldFilter)
            return null;

        if (_tenantProvider.RoleId == RoleAdmin && _tenantProvider.SchoolId is { } schoolId)
            return await _schoolCampusRepository.GetCampusIdsBySchoolAsync(schoolId, ct);

        if (_tenantProvider.CampusId is { } campusId)
            return [campusId];

        return null;
    }
}
