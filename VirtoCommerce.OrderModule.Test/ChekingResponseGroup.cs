using Moq;
using VirtoCommerce.OrderModule.Web.Controllers.Api;
using VirtoCommerce.OrderModule.Web.Security;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    public class ChekingResponseGroup
    {
        [Fact]
        public void CanCheckResponseGroup()
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

            var permissions = new Permission[]
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

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(x => x.GetUserPermissions(It.IsAny<string>())).Returns(permissions);
            var controller = new OrderModuleController(null, null, null, null, null, null, null, securityService.Object, null, null, null, null, null, null);

            Assert.Equal("scope1,scope2", controller.CheckResponseGroup("user", null));
            Assert.Equal("scope1,scope2", controller.CheckResponseGroup("user", "scope1,scope2"));
            Assert.Equal("scope1,scope2", controller.CheckResponseGroup("user", "scope_,scope1,scope2"));
            Assert.Equal("scope1", controller.CheckResponseGroup("user", "scope1"));
            Assert.Equal("", controller.CheckResponseGroup("user", "scope_"));
            Assert.Equal("", controller.CheckResponseGroup("user", "scope_, scope__"));

            securityService.Setup(x => x.GetUserPermissions(It.IsAny<string>())).Returns(new Permission[0]);

            Assert.Equal("scope1,scope2", controller.CheckResponseGroup("user", "scope1,scope2"));
        }
    }
}
