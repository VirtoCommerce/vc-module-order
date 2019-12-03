using System;
using System.Collections.Generic;
using VirtoCommerce.OrderModule.Web.Security;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    [Trait("Category", "CI")]
    [CLSCompliant(false)]
    public class CheckingResponseGroup
    {
        private static Permission[] PreparePermissions(bool withPrices)
        {
            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = OrderPredefinedPermissions.Read,
                },
                new Permission
                {
                    Id = OrderPredefinedPermissions.Access,
                }
            };

            if (withPrices)
            {
                permissions.Add(new Permission
                {
                    Id = OrderPredefinedPermissions.ReadPrices
                });
            }

            return permissions.ToArray();
        }

        [Theory]
        [InlineData("WithItems, WithInPayments, WithShipments, WithAddresses, WithDiscounts", null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        [InlineData("Default", "WithPrices")]
        public void CanCheckPermissionsWithNoPrices(string expected, string respGroup)
        {
            // Arrange
            var permissions = PreparePermissions(false);
            var user = new ApplicationUserExtended();

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("WithPrices,scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        public void CanCheckPermissionsWithPrices(string expected, string respGroup)
        {
            // Arrange
            var permissions = PreparePermissions(true);
            var user = new ApplicationUserExtended();

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("WithPrices,scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        public void CanCheckPermissionsNoPermissions(string expected, string respGroup)
        {
            // Arrange
            var permissions = new Permission[0];
            var user = new ApplicationUserExtended();

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("WithPrices,scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        public void ApplyResponseGroupFiltering_AdminWithOrderPermissionNoReadPrices_NoChangesInResponseGroup(string expected, string respGroup)
        {
            // Arrange
            var permissions = PreparePermissions(false);
            var user = new ApplicationUserExtended()
            {
                IsAdministrator = true,
            };

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("WithPrices,scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        public void ApplyResponseGroupFiltering_AdminWithOrderPermissionWithReadPrices_NoChangesInResponseGroup(string expected, string respGroup)
        {
            // Arrange
            var permissions = PreparePermissions(true);
            var user = new ApplicationUserExtended()
            {
                IsAdministrator = true,
            };

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("scope1,scope2", "scope1,scope2")]
        [InlineData("WithPrices,scope1,scope2", "WithPrices,scope1,scope2")]
        [InlineData("scope1", "scope1")]
        public void ApplyResponseGroupFiltering_AdminNoPermissions_NoChangesInResponseGroup(string expected, string respGroup)
        {
            // Arrange
            var permissions = new Permission[0];
            var user = new ApplicationUserExtended()
            {
                IsAdministrator = true,
            };

            // Act
            var result = OrderReadPricesPermission.ApplyResponseGroupFiltering(user, permissions, respGroup);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
