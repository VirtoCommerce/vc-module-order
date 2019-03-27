using System;
using Moq;
using VirtoCommerce.OrderModule.Web.Controllers.Api;
using VirtoCommerce.OrderModule.Web.Security;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    [Trait("Category", "CI")]
    [CLSCompliant(false)]
    public class CheckingResponseGroup
    {
        private static Permission[] PreparePermissions()
        {
            var scope1 = new OrderLimitResponseScope
            {
                Type = nameof(OrderLimitResponseScope),
                Scope = "scope1"
            };
            var scope2 = new OrderLimitResponseScope
            {
                Type = nameof(OrderLimitResponseScope),
                Scope = "scope2"
            };

            return new Permission[]
            {
                new Permission
                {
                    Id = OrderPredefinedPermissions.Read,
                    AssignedScopes = new PermissionScope[]
                    {
                        scope1,
                        scope2
                    }
                },
                new Permission
                {
                    Id = OrderPredefinedPermissions.Access,
                    AssignedScopes = new PermissionScope [0]
                }
            };
        }

        [Theory]
        [InlineData("scope1,scope2", null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("scope1,scope2", "scope_,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        [InlineData(null, "scope_")]
        [InlineData(null, "scope_,scope__")]
        public void CanCheckResponseGroup(string expected, string scope)
        {
            var permissions = PreparePermissions();

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(x => x.GetUserPermissions(It.IsAny<string>())).Returns(permissions);
            var controller = new OrderModuleController(null, null, null, null, null, null, null, securityService.Object, null, null, null, null, null, null);

            Assert.Equal(expected, controller.GetAllowedResponseGroups("user", scope));
        }

        [Fact]
        public void CanCheckResponseGroupForUserWithNoScope()
        {
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(x => x.GetUserPermissions(It.IsAny<string>())).Returns(new Permission[0]);
            var controller = new OrderModuleController(null, null, null, null, null, null, null, securityService.Object, null, null, null, null, null, null);

            Assert.Equal("scope1,scope2", controller.GetAllowedResponseGroups("user", "scope1,scope2"));
        }
    }
}
