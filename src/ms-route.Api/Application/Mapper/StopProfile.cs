using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

public class StopProfile : Profile
{
    public StopProfile()
    {
        CreateMap<Stop, StopListDto>();

        CreateMap<Stop, StopResponseDto>()
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId.ToString()))
            .ForMember(dest => dest.SchoolId, opt => opt.MapFrom(src => src.SchoolId.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<StopRequestDto, Stop>();
    }
}
