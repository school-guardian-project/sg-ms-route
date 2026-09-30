using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetStopService : IGetStopUseCase
{
    private readonly IStopRepository _repository;

    public GetStopService(IStopRepository repository)
    {
        _repository = repository;
    }

    public async Task<StopResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var stop = await _repository.GetByIdAsync(id, ct);
        if (stop is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        return new StopResponseDto
        {
            Id = stop.Id,
            Address = stop.Address,
            Latitude = stop.Latitude,
            Longitude = stop.Longitude,
            CityId = stop.CityId.ToString(),
            SchoolId = stop.SchoolId.ToString(),
            Status = stop.Status.ToString(),
            CreatedAt = stop.CreatedAt,
            UpdatedAt = stop.UpdatedAt
        };
    }
}
