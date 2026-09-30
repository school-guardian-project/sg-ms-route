using AutoMapper;
using ms_route.Api.Application.Mapper;

namespace ms_route.Tests.Fakes;

public static class TestMapper
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration();
        config.AddProfile<RouteProfile>();
        return config.CreateMapper();
    }
}
