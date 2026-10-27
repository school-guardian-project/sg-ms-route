using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.Mapper;

public class RouteProfile : Profile
{
    public RouteProfile()
    {
        CreateMap<RouteModel, RouteListDto>()
            .ForMember(dest => dest.CampuseId, opt => opt.MapFrom(src => src.CampuseId.ToString()));

        CreateMap<RouteModel, RouteResponseDto>()
            .ForMember(dest => dest.CampuseId, opt => opt.MapFrom(src => src.CampuseId.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<RouteRequestDto, RouteModel>();

        CreateMap<RouteModel, RouteDetailDto>()
            .ForMember(dest => dest.CampuseId, opt => opt.MapFrom(src => src.CampuseId.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek));

        CreateMap<RouteExecution, TripResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<RouteBusAssignment, BusAssignmentDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Bus (ms-fleet, gRPC) de la ruta en las vistas de detalle (FR-9.1/9.2).
        // RouteDetailPopulator lo usa: sin este mapa la vista falla con 500.
        CreateMap<BusInfo, RouteBusDto>();
    }
}