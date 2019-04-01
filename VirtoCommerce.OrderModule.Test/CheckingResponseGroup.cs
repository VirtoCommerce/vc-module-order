using System;
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

            Assert.Equal(expected, OrderLimitResponseScope.GetAllowedResponseGroups(permissions, scope));
        }

        [Fact]
        public void CanCheckResponseGroupForUserWithNoScope()
        {
            Assert.Equal("scope1,scope2", OrderLimitResponseScope.GetAllowedResponseGroups(new Permission[0], "scope1,scope2"));
        }
    }
}
