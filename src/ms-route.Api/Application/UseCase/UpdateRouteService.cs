using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class UpdateRouteService : IUpdateRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IRouteScheduleProvisioner _scheduleProvisioner;
    private readonly IMapper _mapper;

    public UpdateRouteService(
        IRouteRepository repository,
        IRouteBusAssignmentRepository assignmentRepository,
        IRouteScheduleProvisioner scheduleProvisioner,
        IMapper mapper)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _scheduleProvisioner = scheduleProvisioner;
        _mapper = mapper;
    }

    public async Task ExecuteAsync(Guid id, RouteRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null)
            throw new InvalidOperationException($"Route not found: {id}");

        _mapper.Map(request, existing);

        await _repository.UpdateAsync(existing, ct);

        // Una ruta con bus siempre tiene horarios con su hora de salida vigente: se
        // crean los que falten (rutas asignadas antes de esta regla) y se alinea la
        // hora de los existentes para que la ventana de inicio no use una hora vieja.
        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(id, ct);
        if (assignment is null) return;

        await _scheduleProvisioner.EnsureSchedulesAsync(
            assignment.Id, existing.StartTime, RouteScheduleProvisioner.WorkingDays, ct);
        await _scheduleProvisioner.SyncStartTimeAsync(assignment.Id, existing.StartTime, ct);
    }
}
