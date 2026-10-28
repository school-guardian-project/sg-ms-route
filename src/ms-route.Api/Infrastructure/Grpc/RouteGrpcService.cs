using Grpc.Core;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using RouteStatus = ms_route.Api.Domain.Model.Status;
using static ms_route.Api.Infrastructure.Grpc.Server.RouteGrpcMapper;

namespace ms_route.Api.Infrastructure.Grpc.Server;

public class RouteGrpcService : RouteService.RouteServiceBase
{
    private readonly ICreateRouteUseCase _createUseCase;
    private readonly IGetRouteUseCase _getUseCase;
    private readonly IGetRouteByExecutionIdUseCase _getRouteByExecutionIdUseCase;
    private readonly IListRouteUseCase _listUseCase;
    private readonly IUpdateRouteUseCase _updateUseCase;
    private readonly IDeleteRouteUseCase _deleteUseCase;
    private readonly IGetCurrentRouteUseCase _getCurrentRouteUseCase;
    private readonly IGetStudentRouteUseCase _getStudentRouteUseCase;
    private readonly IStartTripUseCase _startTripUseCase;
    private readonly IEndTripUseCase _endTripUseCase;
    private readonly IAssignStudentToRouteUseCase _assignStudentUseCase;
    private readonly IAssignBusToRouteUseCase _assignBusUseCase;
    private readonly IAttachStopToRouteUseCase _attachStopUseCase;
    private readonly IGetStudentStopOnRouteUseCase _getStudentStopOnRouteUseCase;
    private readonly IGetCurrentTripUseCase _getCurrentTripUseCase;
    private readonly IGetTripStatusUseCase _getTripStatusUseCase;

    public RouteGrpcService(
        ICreateRouteUseCase createUseCase,
        IGetRouteUseCase getUseCase,
        IGetRouteByExecutionIdUseCase getRouteByExecutionIdUseCase,
        IListRouteUseCase listUseCase,
        IUpdateRouteUseCase updateUseCase,
        IDeleteRouteUseCase deleteUseCase,
        IGetCurrentRouteUseCase getCurrentRouteUseCase,
        IGetStudentRouteUseCase getStudentRouteUseCase,
        IStartTripUseCase startTripUseCase,
        IEndTripUseCase endTripUseCase,
        IAssignStudentToRouteUseCase assignStudentUseCase,
        IAssignBusToRouteUseCase assignBusUseCase,
        IAttachStopToRouteUseCase attachStopUseCase,
        IGetStudentStopOnRouteUseCase getStudentStopOnRouteUseCase,
        IGetCurrentTripUseCase getCurrentTripUseCase,
        IGetTripStatusUseCase getTripStatusUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _getRouteByExecutionIdUseCase = getRouteByExecutionIdUseCase;
        _listUseCase = listUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
        _getCurrentRouteUseCase = getCurrentRouteUseCase;
        _getStudentRouteUseCase = getStudentRouteUseCase;
        _startTripUseCase = startTripUseCase;
        _endTripUseCase = endTripUseCase;
        _assignStudentUseCase = assignStudentUseCase;
        _assignBusUseCase = assignBusUseCase;
        _attachStopUseCase = attachStopUseCase;
        _getStudentStopOnRouteUseCase = getStudentStopOnRouteUseCase;
        _getCurrentTripUseCase = getCurrentTripUseCase;
        _getTripStatusUseCase = getTripStatusUseCase;
    }

    public override async Task<CreateRouteResponse> CreateRoute(
        CreateRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _createUseCase.ExecuteAsync(
                ToRouteRequestDto(request), context.CancellationToken);

            return ToCreateRouteResponse(result);
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

            var response = new GetRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                TargetSector = result.TargetSector,
                Status = result.Status,
                CampuseId = result.CampuseId,
                StartTime = result.StartTime.ToString("HH:mm:ss"),
                EndTime = result.EndTime.ToString("HH:mm:ss")
            };
            response.DaysOfWeek.AddRange(result.DaysOfWeek);
            return response;
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
        response.Routes.AddRange(routes.Select(r =>
        {
            var item = new RouteListItem
            {
                Id = r.Id.ToString(),
                Name = r.Name
            };
            item.DaysOfWeek.AddRange(r.DaysOfWeek);
            return item;
        }));

        return response;
    }

    public override async Task<UpdateRouteResponse> UpdateRoute(
        UpdateRouteRequest request, ServerCallContext context)
    {
        try
        {
            await _updateUseCase.ExecuteAsync(
                ParseId(request.Id, "route_id"), ToRouteRequestDto(request), context.CancellationToken);

            var response = new UpdateRouteResponse
            {
                Id = request.Id,
                Name = request.Name,
                TargetSector = request.TargetSector,
                Status = RouteStatus.Active.ToString(),
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };
            response.DaysOfWeek.AddRange(request.DaysOfWeek);
            return response;
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

            var response = new GetCurrentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = result.Status,
                StartTime = result.StartTime.ToString("HH:mm:ss"),
                EndTime = result.EndTime.ToString("HH:mm:ss")
            };
            response.Stops.AddRange(ToProtoStops(result.Stops));
            return response;
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

            // Sin ruta asignada: NotFound, igual que en REST. Antes esto era una
            // excepcion y el RPC devolvia Unknown.
            if (result is null)
            {
                throw new RpcException(new global::Grpc.Core.Status(
                    StatusCode.NotFound, "student has no active route assigned"));
            }

            var response = new GetStudentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = result.Status,
                StartTime = result.StartTime.ToString("HH:mm:ss"),
                EndTime = result.EndTime.ToString("HH:mm:ss")
            };
            response.Stops.AddRange(ToProtoStops(result.Stops));
            response.Schedules.AddRange(ToProtoSchedules(result.Schedules));
            return response;
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
                Status = result.Status,
                Direction = result.Direction
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
                Status = result.Status,
                Direction = result.Direction
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

    public override async Task<AttachStopToRouteResponse> AttachStopToRoute(
        AttachStopToRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _attachStopUseCase.ExecuteAsync(
                ParseId(request.RouteId, "route_id"),
                ParseId(request.StopId, "stop_id"),
                context.CancellationToken);

            return new AttachStopToRouteResponse
            {
                Id = result.Id.ToString(),
                RouteId = result.RouteId.ToString(),
                StopId = result.StopId.ToString(),
                OrderSequence = result.OrderSequence,
                Status = result.Status
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<GetStudentStopOnRouteResponse> GetStudentStopOnRoute(
        GetStudentStopOnRouteRequest request, ServerCallContext context)
    {
        // ParseId lanza InvalidArgument si el UUID es invalido.
        var result = await _getStudentStopOnRouteUseCase.ExecuteAsync(
            ParseId(request.RouteId, "route_id"),
            ParseId(request.StudentProfileId, "student_profile_id"),
            context.CancellationToken);

        if (result is null)
        {
            return new GetStudentStopOnRouteResponse { Found = false };
        }

        return new GetStudentStopOnRouteResponse
        {
            Found = true,
            RouteId = result.RouteId.ToString(),
            StudentProfileId = result.StudentProfileId.ToString(),
            RouteStopId = result.RouteStopId.ToString(),
            StopId = result.StopId.ToString(),
            StopName = result.StopName,
            StopAddress = result.StopAddress,
            Latitude = (double)result.Latitude,
            Longitude = (double)result.Longitude
        };
    }

    public override async Task<GetCurrentTripResponse> GetCurrentTrip(
        GetCurrentTripRequest request, ServerCallContext context)
    {
        var result = await _getCurrentTripUseCase.ExecuteAsync(
            ParseId(request.DriverId, "driver_id"), context.CancellationToken);

        if (result is null)
        {
            return new GetCurrentTripResponse { Found = false };
        }

        return new GetCurrentTripResponse
        {
            Found = true,
            Id = result.Id.ToString(),
            RouteId = result.RouteId.ToString(),
            BusId = result.BusId.ToString(),
            DriverId = result.DriverId.ToString(),
            Status = result.Status,
            Direction = result.Direction
        };
    }

    public override async Task<GetRouteByExecutionIdResponse> GetRouteByExecutionId(
        GetRouteByExecutionIdRequest request, ServerCallContext context)
    {
        var route = await _getRouteByExecutionIdUseCase.ExecuteAsync(
            ParseId(request.ExecutionId, "execution_id"), context.CancellationToken);
        return route is null
            ? new GetRouteByExecutionIdResponse { Found = false }
            : new GetRouteByExecutionIdResponse
            {
                Found = true,
                RouteId = route.Id.ToString(),
                RouteName = route.Name
            };
    }

    public override async Task<GetTripStatusResponse> GetTripStatus(
        GetTripStatusRequest request, ServerCallContext context)
    {
        try
        {
            var driverId = ParseId(request.DriverId, "driver_id");
            var result = await _getTripStatusUseCase.ExecuteAsync(driverId, context.CancellationToken);

            if (result is null)
            {
                return new GetTripStatusResponse { Found = false };
            }

            var response = new GetTripStatusResponse
            {
                Found = true,
                TripId = result.TripId.ToString(),
                RouteId = result.RouteId.ToString(),
                RouteName = result.RouteName,
                Direction = result.Direction,
                Status = (int)result.Status,
                CanStart = result.CanStart,
                CanEnd = result.CanEnd,
                Message = result.Message,
                ScheduleStartTime = result.ScheduleStartTime.ToString("HH:mm:ss"),
                NextScheduleTime = result.NextScheduleTime?.ToString("HH:mm:ss") ?? string.Empty
            };

            if (result.EndConditionDetail is not null)
            {
                response.EndConditionDetail = new TripEndConditionDetail
                {
                    CanEnd = result.EndConditionDetail.CanEnd,
                    TotalAssignedStudents = result.EndConditionDetail.TotalAssignedStudents,
                    StudentsAlightedAtFinalStop = result.EndConditionDetail.StudentsAlightedAtFinalStop,
                    IsFinalStopReached = result.EndConditionDetail.IsFinalStopReached,
                    FinalStopName = result.EndConditionDetail.FinalStopName,
                    Message = result.EndConditionDetail.Message
                };
            }

            return response;
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new global::Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

}
