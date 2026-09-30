using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetStudentRouteService : IGetStudentRouteUseCase
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMapper _mapper;

    public GetStudentRouteService(IRouteRepository routeRepository, IMapper mapper)
    {
        _routeRepository = routeRepository;
        _mapper = mapper;
    }

    public async Task<RouteDetailDto> ExecuteAsync(Guid studentId, CancellationToken ct = default)
    {
        var routes = await _routeRepository.GetAllAsync(ct);
        var route = routes.FirstOrDefault();

        if (route is null)
            throw new InvalidOperationException("No route assigned yet");

        return _mapper.Map<RouteDetailDto>(route);
    }
}
