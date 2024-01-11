using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    public class LogChangesOrderTests
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public void GetChangedEntryOperationLogsTest(CustomerOrder newOrder, CustomerOrder oldOrder, EntryState state, OperationLog expectedResult)
        {
            // Arrange
            if (newOrder?.Addresses == null)
            {
                newOrder.Addresses = new List<Core.Model.Address>();
            }

            if (oldOrder?.Addresses == null)
            {
                oldOrder.Addresses = new List<Core.Model.Address>();
            }

            var changedEntry = new GenericChangedEntry<IOperation>(newOrder, oldOrder, state);

            var changeLogServiceMock = new Mock<IChangeLogService>();
            var memberServiceMock = new Mock<IMemberService>();
            var settingsManagerMock = new Mock<ISettingsManager>();

            memberServiceMock.Setup(x => x.GetByIdAsync($"{NewOrderTemplate.EmployeeId}", null, null))
                .ReturnsAsync(new Employee() { FullName = $"{NewOrderTemplate.EmployeeId}" });
            memberServiceMock.Setup(x => x.GetByIdAsync($"{OldOrderTemplate.EmployeeId}", null, null))
                .ReturnsAsync(new Employee() { FullName = $"{OldOrderTemplate.EmployeeId}" });

            var handler = new LogChangesOrderChangedEventHandlerStub(changeLogServiceMock.Object, memberServiceMock.Object, settingsManagerMock.Object);

            // Act
            var result = handler.GetChangedEntryOperationLogsMock(changedEntry);

            // Assert
            result.Should().HaveCount(1);
            result.First().ObjectId.Should().Be(expectedResult.ObjectId);
            result.First().OperationType.Should().Be(expectedResult.OperationType);
            result.First().Detail.Should().Be(expectedResult.Detail);
        }


        private static readonly CustomerOrder NewOrderTemplate = new CustomerOrder
        {
            Id = "Order1",
            Number = "NumberNew",
            EmployeeId = "EmployeeNew",
        };

        private static readonly CustomerOrder OldOrderTemplate = new CustomerOrder
        {
            Id = "Order1",
            Number = "NumberOld",
            EmployeeId = "EmployeeOld",
        };

        public static readonly IList<object[]> TestData = new List<object[]>
        {
            // add order
            new object[]
            {
                new CustomerOrder()
                {
                    Id = NewOrderTemplate.Id,
                    Number = NewOrderTemplate.Number,
                },
                new CustomerOrder(),
                EntryState.Added,
                new OperationLog()
                {
                    ObjectId = NewOrderTemplate.Id,
                    ObjectType = nameof(CustomerOrder),
                    OperationType = EntryState.Added,
                    Detail = $"The new CustomerOrder {NewOrderTemplate.Number} added",
                }
            },
            // delete order
            new object[]
            {
                new CustomerOrder()
                {
                    Id = NewOrderTemplate.Id,
                    Number = NewOrderTemplate.Number,
                },
                new CustomerOrder(),
                EntryState.Deleted,
                new OperationLog()
                {
                    ObjectId = NewOrderTemplate.Id,
                    ObjectType = nameof(CustomerOrder),
                    OperationType = EntryState.Deleted,
                    Detail = $"The CustomerOrder {NewOrderTemplate.Number} deleted",
                }
            },
            // change employee
            new object[]
            {
                new CustomerOrder()
                {
                    Id = NewOrderTemplate.Id,
                    EmployeeId = NewOrderTemplate.EmployeeId,
                },
                new CustomerOrder()
                {
                    Id = OldOrderTemplate.Id,
                    EmployeeId = OldOrderTemplate.EmployeeId,
                },
                EntryState.Modified,
                new OperationLog()
                {
                    ObjectId = NewOrderTemplate.Id,
                    ObjectType = nameof(CustomerOrder),
                    OperationType = EntryState.Modified,
                    Detail = $"Order employee was changed to '{NewOrderTemplate.EmployeeId}'",
                }
            },
            // change number
            new object[]
            {
                new CustomerOrder()
                {
                    Id = NewOrderTemplate.Id,
                    Number = NewOrderTemplate.Number,
                },
                new CustomerOrder()
                {
                    Id = OldOrderTemplate.Id,
                    Number = OldOrderTemplate.Number,
                },
                EntryState.Modified,
                new OperationLog()
                {
                    ObjectId = NewOrderTemplate.Id,
                    ObjectType = nameof(CustomerOrder),
                    OperationType = EntryState.Modified,
                    Detail = $"The CustomerOrder {NewOrderTemplate.Number} property 'Number' changed from '{OldOrderTemplate.Number}' to '{NewOrderTemplate.Number}'",
                }
            },
        };



        /// <summary>
        /// Stub class to expose protected methods for testing
        /// </summary>
        public class LogChangesOrderChangedEventHandlerStub : LogChangesOrderChangedEventHandler
        {
            public LogChangesOrderChangedEventHandlerStub(IChangeLogService changeLogService, IMemberService memberService, ISettingsManager settingsManager)
                : base(changeLogService, memberService, settingsManager)
            {
            }

            public IEnumerable<OperationLog> GetChangedEntryOperationLogsMock(GenericChangedEntry<IOperation> changedEntry)
            {
                return base.GetChangedEntryOperationLogs(changedEntry);
            }
        }
    }
}
