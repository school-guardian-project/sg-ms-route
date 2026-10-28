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

        var requestedDays = request.DaysOfWeek is { Count: > 0 }
            ? request.DaysOfWeek.ToList()
            : existing.DaysOfWeek.ToList();
        var scheduleDays = RouteScheduleProvisioner.ParseConfiguredDays(requestedDays);
        var daysToPersist = requestedDays.Count > 0
            ? scheduleDays.Select(day => day.ToString()).ToList()
            : [];

        _mapper.Map(request, existing);
        existing.DaysOfWeek = daysToPersist;

        await _repository.UpdateAsync(existing, ct);

        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(id, ct);
        if (assignment is null) return;

        await _scheduleProvisioner.SyncSchedulesAsync(
            assignment.Id,
            existing.StartTime,
            scheduleDays,
            ct);
    }
}
