using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class UpdateStopService : IUpdateStopUseCase
{
    private readonly IStopRepository _repository;

    public UpdateStopService(IStopRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id, StopRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Address is required.", nameof(request.Address));

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        existing.Name = request.Name.Trim();
        existing.CityId = request.CityId;
        existing.SchoolId = request.SchoolId;
        existing.Address = request.Address.Trim();
        existing.Longitude = request.Longitude;
        existing.Latitude = request.Latitude;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, ct);
    }
}
