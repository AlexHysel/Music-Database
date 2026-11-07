using System.Net;
using MusicDatabase.Core.Services;

namespace MusicDatabase.Tests.UnitTests;

public class RedisConnectionFactoryTests
{
    [Fact]
    public void CreateConfiguration_ShouldRetryAndAllowConnectionRecovery()
    {
        var options = CacheService.CreateConfiguration("redis:6379,abortConnect=false");
        var endpoint = Assert.IsType<DnsEndPoint>(options.EndPoints.Single());

        Assert.False(options.AbortOnConnectFail);
        Assert.True(options.ConnectRetry > 0);
        Assert.Equal("redis", endpoint.Host);
        Assert.Equal(6379, endpoint.Port);
    }
}
