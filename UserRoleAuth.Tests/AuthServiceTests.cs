using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using UserRoleAuth.Infrastructure.Services;

namespace UserRoleAuth.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public void GenerateRefreshToken_Returns_String()
        {
            var inMemorySettings = new Dictionary<string, string>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var svc = new JwtService(configuration);
            var token = svc.GenerateRefreshToken();
            token.Should().NotBeNullOrEmpty();
        }
    }
}
