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
                CampuseId = Guid.Parse(request.CampuseId),
                Name = request.Name,
                TargetSector = request.TargetSector
            }, context.CancellationToken);

            return new CreateRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                TargetSector = result.TargetSector,
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<GetRouteResponse> GetRoute(
        GetRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getUseCase.ExecuteAsync(Guid.Parse(request.Id), context.CancellationToken);

            return new GetRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                TargetSector = result.TargetSector,
                Status = RouteStatus.Active.ToString(),
                CampuseId = result.CampuseId
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
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
            await _updateUseCase.ExecuteAsync(Guid.Parse(request.Id), new RouteRequestDto
            {
                CampuseId = Guid.Parse(request.CampuseId),
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
            throw new RpcException(new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<DeleteRouteResponse> DeleteRoute(
        DeleteRouteRequest request, ServerCallContext context)
    {
        try
        {
            await _deleteUseCase.ExecuteAsync(Guid.Parse(request.Id), context.CancellationToken);

            return new DeleteRouteResponse { Success = true };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<GetCurrentRouteResponse> GetCurrentRoute(
        GetCurrentRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getCurrentRouteUseCase.ExecuteAsync(Guid.Parse(request.DriverId), context.CancellationToken);

            return new GetCurrentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<GetStudentRouteResponse> GetStudentRoute(
        GetStudentRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _getStudentRouteUseCase.ExecuteAsync(Guid.Parse(request.StudentId), context.CancellationToken);

            return new GetStudentRouteResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                CampuseId = result.CampuseId,
                TargetSector = result.TargetSector,
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<StartTripResponse> StartTrip(
        StartTripRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _startTripUseCase.ExecuteAsync(
                Guid.Parse(request.RouteId),
                Guid.Parse(request.BusId),
                Guid.Parse(request.DriverId),
                context.CancellationToken);

            return new StartTripResponse
            {
                Id = result.Id.ToString(),
                BusId = result.BusId.ToString(),
                DriverId = result.DriverId.ToString(),
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<EndTripResponse> EndTrip(
        EndTripRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _endTripUseCase.ExecuteAsync(Guid.Parse(request.TripId), context.CancellationToken);

            return new EndTripResponse
            {
                Id = result.Id.ToString(),
                BusId = result.BusId.ToString(),
                DriverId = result.DriverId.ToString(),
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<AssignStudentToRouteResponse> AssignStudentToRoute(
        AssignStudentToRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _assignStudentUseCase.ExecuteAsync(
                Guid.Parse(request.RouteId),
                Guid.Parse(request.StudentId),
                Guid.Parse(request.StopId),
                context.CancellationToken);

            return new AssignStudentToRouteResponse
            {
                Id = result.Id.ToString(),
                RouteId = result.RouteId.ToString(),
                StudentId = result.StudentId.ToString(),
                StopId = result.StopId.ToString(),
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<AssignBusToRouteResponse> AssignBusToRoute(
        AssignBusToRouteRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _assignBusUseCase.ExecuteAsync(
                Guid.Parse(request.RouteId),
                Guid.Parse(request.BusId),
                context.CancellationToken);

            return new AssignBusToRouteResponse
            {
                Id = result.Id.ToString(),
                RouteId = result.RouteId.ToString(),
                BusId = result.BusId.ToString(),
                Status = RouteStatus.Active.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }
}
