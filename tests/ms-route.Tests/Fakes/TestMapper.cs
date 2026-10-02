using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ms_route.Api.Application.Mapper;

namespace ms_route.Tests.Fakes;

public static class TestMapper
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<RouteProfile>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }
}
