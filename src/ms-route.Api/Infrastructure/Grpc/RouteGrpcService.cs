using Grpc.Core;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using RouteStatus = ms_route.Api.Domain.Model.Status;

namespace ms_route.Api.Infrastructure.Grpc;

public class RouteGrpcService : RouteService.RouteServiceBase
{
    private readonly ICreateRouteUseCase _createUseCase;
    private readonly IGetRouteUseCase _getUseCase;
    private readonly IListRouteUseCase _listUseCase;
    private readonly IUpdateRouteUseCase _updateUseCase;
    private readonly IDeleteRouteUseCase _deleteUseCase;
    private readonly IGetCurrentRouteUseCase _getCurrentRouteUseCase;
    private readonly IGetStudentRouteUseCase _getStudentRouteUseCase;
    private readonly IStartTripUseCase _startTripUseCase;
    private readonly IEndTripUseCase _endTripUseCase;
    private readonly IAssignStudentToRouteUseCase _assignStudentUseCase;
    private readonly IAssignBusToRouteUseCase _assignBusUseCase;

    public RouteGrpcService(
        ICreateRouteUseCase createUseCase,
        IGetRouteUseCase getUseCase,
        IListRouteUseCase listUseCase,
        IUpdateRouteUseCase updateUseCase,
        IDeleteRouteUseCase deleteUseCase,
        IGetCurrentRouteUseCase getCurrentRouteUseCase,
        IGetStudentRouteUseCase getStudentRouteUseCase,
        IStartTripUseCase startTripUseCase,
        IEndTripUseCase endTripUseCase,
        IAssignStudentToRouteUseCase assignStudentUseCase,
        IAssignBusToRouteUseCase assignBusUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _listUseCase = listUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
        _getCurrentRouteUseCase = getCurrentRouteUseCase;
        _getStudentRouteUseCase = getStudentRouteUseCase;
        _startTripUseCase = startTripUseCase;
        _endTripUseCase = endTripUseCase;
        _assignStudentUseCase = assignStudentUseCase;
        _assignBusUseCase = assignBusUseCase;
    }

    public override async Task<CreateRouteResponse> CreateRoute(
        CreateRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _createUseCase.ExecuteAsync(new RouteRequestDto
            {
                CampuseId = ParseId(request.CampuseId, "campuse_id"),
                Name = request.Name,
                TargetSector = request.TargetSector
            }, context.CancellationToken);

            return new CreateRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                TargetSector = result.TargetSector,
                Status = result.Status
            };
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<GetRouteResponse> GetRoute(
        GetRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getUseCase.ExecuteAsync(ParseId(request.Id, "route_id"), context.CancellationToken);

            return new GetRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                TargetSector = result.TargetSector,
                Status = result.Status,
                CampuseId = result.CampuseId
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ListRoutesResponse> ListRoutes(
        ListRoutesRequest request, ServerCallContext context)
    {
        var routes = await _listUseCase.ExecuteAsync(context.CancellationToken);

        var response = new ListRoutesResponse();
        response.Routes.AddRange(routes.Select(r => new RouteListItem
        {
            Id = r.Id.ToString(),
            Name = r.Name
        }));

        return response;
    }

    public override async Task<UpdateRouteResponse> UpdateRoute(
        UpdateRouteRequest request, ServerCallContext context)
    {
        try
        {
            await _updateUseCase.ExecuteAsync(ParseId(request.Id, "route_id"), new RouteRequestDto
            {
                CampuseId = ParseId(request.CampuseId, "campuse_id"),
                Name = request.Name,
                TargetSector = request.TargetSector
            }, context.CancellationToken);

            return new UpdateRouteResponse
            {
                Id = request.Id,
                Name = request.Name,
                TargetSector = request.TargetSector,
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<DeleteRouteResponse> DeleteRoute(
        DeleteRouteRequest request, ServerCallContext context)
    {
        try
        {
            await _deleteUseCase.ExecuteAsync(ParseId(request.Id, "route_id"), context.CancellationToken);

            return new DeleteRouteResponse { Success = true };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<GetCurrentRouteResponse> GetCurrentRoute(
        GetCurrentRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getCurrentRouteUseCase.ExecuteAsync(ParseId(request.DriverId, "driver_id"), context.CancellationToken);

            return new GetCurrentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<GetStudentRouteResponse> GetStudentRoute(
        GetStudentRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getStudentRouteUseCase.ExecuteAsync(ParseId(request.StudentId, "student_id"), context.CancellationToken);

            return new GetStudentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<StartTripResponse> StartTrip(
        StartTripRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _startTripUseCase.ExecuteAsync(
                ParseId(request.RouteId, "route_id"),
                ParseId(request.BusId, "bus_id"),
                ParseId(request.DriverId, "driver_id"),
                context.CancellationToken);

            return new StartTripResponse
            {
                Id = result.Id.ToString(),
                BusId = result.BusId.ToString(),
                DriverId = result.DriverId.ToString(),
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<EndTripResponse> EndTrip(
        EndTripRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _endTripUseCase.ExecuteAsync(ParseId(request.TripId, "trip_id"), context.CancellationToken);

            return new EndTripResponse
            {
                Id = result.Id.ToString(),
                BusId = result.BusId.ToString(),
                DriverId = result.DriverId.ToString(),
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<AssignStudentToRouteResponse> AssignStudentToRoute(
        AssignStudentToRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _assignStudentUseCase.ExecuteAsync(
                ParseId(request.RouteId, "route_id"),
                ParseId(request.StudentId, "student_id"),
                ParseId(request.StopId, "stop_id"),
                context.CancellationToken);

            return new AssignStudentToRouteResponse
            {
                Id = result.Id.ToString(),
                RouteId = result.RouteId.ToString(),
                StudentId = result.StudentId.ToString(),
                StopId = result.StopId.ToString(),
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<AssignBusToRouteResponse> AssignBusToRoute(
        AssignBusToRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _assignBusUseCase.ExecuteAsync(
                ParseId(request.RouteId, "route_id"),
                ParseId(request.BusId, "bus_id"),
                context.CancellationToken);

            return new AssignBusToRouteResponse
            {
                Id = result.Id.ToString(),
                RouteId = result.RouteId.ToString(),
                BusId = result.BusId.ToString(),
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    private static Guid ParseId(string value, string field)
    {
        if (!Guid.TryParse(value, out var id))
            throw new RpcException(new global::Grpc.Core.Status(
                StatusCode.InvalidArgument, $"Invalid {field}, use UUID: {value}"));
        return id;
    }
}
