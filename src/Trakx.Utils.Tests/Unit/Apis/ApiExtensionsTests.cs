using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Trakx.Utils.Apis;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Apis
{
    public class ApiExtensionsTests
    {
        [Fact]
        public void ServiceCollection_add_versioning()
        {
            var services = new ServiceCollection();
            services.AddVersioning("0.1");
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.Should().NotBeNull();
        }
    }
}
