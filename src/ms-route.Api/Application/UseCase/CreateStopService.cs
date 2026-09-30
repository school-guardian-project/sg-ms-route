using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class CreateStopService : ICreateStopUseCase
{
    private readonly IStopRepository _repository;

    public CreateStopService(IStopRepository repository)
    {
        _repository = repository;
    }

    public async Task<StopResponseDto> ExecuteAsync(StopRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Address is required.", nameof(request.Address));

        var stop = new Stop
        {
            Id = Guid.NewGuid(),
            CityId = request.CityId,
            SchoolId = request.SchoolId,
            Address = request.Address.Trim(),
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            Status = StopStatus.Active
        };

        var saved = await _repository.SaveAsync(stop, ct);

        return new StopResponseDto
        {
            Id = saved.Id,
            Address = saved.Address,
            Latitude = saved.Latitude,
            Longitude = saved.Longitude,
            CityId = saved.CityId.ToString(),
            SchoolId = saved.SchoolId.ToString(),
            Status = saved.Status.ToString(),
            CreatedAt = saved.CreatedAt,
            UpdatedAt = saved.UpdatedAt
        };
    }
}
