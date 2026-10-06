using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class CreateStopService : ICreateStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IMapper _mapper;

    public CreateStopService(IStopRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StopResponseDto> ExecuteAsync(StopRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Address is required.", nameof(request.Address));

        var stop = _mapper.Map<Stop>(request);
        stop.Id = Guid.NewGuid();
        stop.Status = Status.Active;

        var saved = await _repository.SaveAsync(stop, ct);

        return _mapper.Map<StopResponseDto>(saved);
    }
}
