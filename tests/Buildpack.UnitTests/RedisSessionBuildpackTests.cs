using Pivotal.Redis.Aspnet.Session.Buildpack;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Buildpack.UnitTests
{
    public class RedisSessionBuildpackTests
    {
        [Fact]
        public void Test_IfDerivedFrom()
        {
            var buildpack = new RedisSessionBuildpack();
            Assert.IsAssignableFrom<SupplyBuildpack>(buildpack);
        }
    }
}
