using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class UpdateStopService : IUpdateStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IMapper _mapper;

    public UpdateStopService(IStopRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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

        _mapper.Map(request, existing);
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, ct);
    }
}
