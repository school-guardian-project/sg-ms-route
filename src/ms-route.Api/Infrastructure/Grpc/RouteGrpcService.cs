using Grpc.Core;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Grpc;

public class RouteGrpcService : RouteService.RouteServiceBase
{
    private readonly ICreateRouteUseCase _createUseCase;
    private readonly IGetRouteUseCase _getUseCase;
    private readonly IListRouteUseCase _listUseCase;
    private readonly IUpdateRouteUseCase _updateUseCase;
    private readonly IDeleteRouteUseCase _deleteUseCase;

    public RouteGrpcService(
        ICreateRouteUseCase createUseCase,
        IGetRouteUseCase getUseCase,
        IListRouteUseCase listUseCase,
        IUpdateRouteUseCase updateUseCase,
        IDeleteRouteUseCase deleteUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _listUseCase = listUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
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
                TargetSector = request.TargetSector,
                StartTime = TimeOnly.FromTimeSpan(request.StartTime),
                EndTime = TimeOnly.FromTimeSpan(request.EndTime)
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
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
                Status = result.Status,
                CampuseId = result.CampuseId
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
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
            Name = r.Name,
            TargetSector = r.TargetSector
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
                TargetSector = request.TargetSector,
                StartTime = TimeOnly.FromTimeSpan(request.StartTime),
                EndTime = TimeOnly.FromTimeSpan(request.EndTime)
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
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
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}
