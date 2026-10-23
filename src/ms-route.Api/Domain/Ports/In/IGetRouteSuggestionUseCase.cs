using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetRouteSuggestionUseCase
{
    Task<RouteSuggestionDto> ExecuteAsync(Guid routeId, CancellationToken ct = default);
}
