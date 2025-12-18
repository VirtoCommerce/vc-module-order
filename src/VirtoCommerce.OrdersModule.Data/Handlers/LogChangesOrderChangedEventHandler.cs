using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoCompare;
using Hangfire;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class LogChangesOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IMemberService _memberService;
        private readonly IChangeLogService _changeLogService;
        private readonly ISettingsManager _settingsManager;
        private static readonly ConcurrentDictionary<Type, List<string>> _auditablePropertiesCacheByTypeDict = new();

        public LogChangesOrderChangedEventHandler(IChangeLogService changeLogService, IMemberService memberService, ISettingsManager settingsManager)
        {
            _changeLogService = changeLogService;
            _memberService = memberService;
            _settingsManager = settingsManager;
        }

        public virtual async Task Handle(OrderChangedEvent message)
        {
            var logOrderChangesEnabled = await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.LogOrderChanges);

            if (logOrderChangesEnabled && message.ChangedEntries.Any())
            {
                var operationLogs = GetOperationLogs(message.ChangedEntries);

                if (!operationLogs.IsNullOrEmpty())
                {
                    BackgroundJob.Enqueue(() => TryToLogChangesBackgroundJob(operationLogs.ToArray()));
                }
            }
        }

        protected virtual IList<OperationLog> GetOperationLogs(IEnumerable<GenericChangedEntry<CustomerOrder>> changedEntries)
        {
            var operationLogs = new List<OperationLog>();

            foreach (var changedEntry in changedEntries)
            {
                switch (changedEntry.EntryState)
                {
                    case EntryState.Modified:
                        {
                            var originalOperations = changedEntry.OldEntry.GetFlatObjectsListWithInterface<IOperation>().Distinct().ToList();
                            var modifiedOperations = changedEntry.NewEntry.GetFlatObjectsListWithInterface<IOperation>().Distinct().ToList();

                            modifiedOperations.CompareTo(originalOperations, EqualityComparer<IOperation>.Default,
                                (state, modified, original) => operationLogs.AddRange(GetChangedEntryOperationLogs(new GenericChangedEntry<IOperation>(modified, original, state))));
                            break;
                        }
                    case EntryState.Added or EntryState.Deleted:
                        operationLogs.AddRange(GetChangedEntryOperationLogs(new GenericChangedEntry<IOperation>(changedEntry.NewEntry, changedEntry.OldEntry, changedEntry.EntryState)));
                        break;
                }
            }

            return operationLogs;
        }

        public Task TryToLogChangesBackgroundJob(OperationLog[] operationLogs)
        {
            return _changeLogService.SaveChangesAsync(operationLogs);
        }

        protected virtual IEnumerable<OperationLog> GetChangedEntryOperationLogs(GenericChangedEntry<IOperation> changedEntry)
        {
            var result = new List<OperationLog>();

            switch (changedEntry.EntryState)
            {
                case EntryState.Modified:
                    {
                        var logs = new List<string>();
                        var diff = GetOperationDifferences(changedEntry, logs);
                        var auditableProperties = GetAuditableProperties(changedEntry);

                        if (auditableProperties.Count > 0)
                        {
                            var observedDifferences = diff
                                .Where(x => auditableProperties.ContainsIgnoreCase(x.Name))
                                .Distinct(new DifferenceComparer());

                            foreach (var difference in observedDifferences)
                            {
                                logs.Add($"The {changedEntry.OldEntry.OperationType} {changedEntry.NewEntry.Number} property '{difference.Name}' changed from '{difference.OldValue}' to '{difference.NewValue}'");
                            }
                        }

                        foreach (var log in logs)
                        {
                            result.Add(GetLogRecord(changedEntry.NewEntry, log));
                        }

                        break;
                    }
                case EntryState.Deleted:
                    {
                        var record = GetLogRecord(changedEntry.NewEntry,
                            $"The {changedEntry.NewEntry.OperationType} {changedEntry.NewEntry.Number} deleted",
                            EntryState.Deleted);
                        result.Add(record);
                        break;
                    }
                case EntryState.Added:
                    {
                        var record = GetLogRecord(changedEntry.NewEntry,
                            $"The new {changedEntry.NewEntry.OperationType} {changedEntry.NewEntry.Number} added",
                            EntryState.Added);
                        result.Add(record);
                        break;
                    }
            }

            return result;
        }

        protected static List<string> GetAuditableProperties(GenericChangedEntry<IOperation> changedEntry)
        {
            var type = changedEntry.OldEntry.GetType();

            return _auditablePropertiesCacheByTypeDict.GetOrAdd(type, t =>
                t.GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, typeof(AuditableAttribute)))
                    .Select(x => x.Name)
                    .ToList());
        }

        protected virtual IList<Difference> GetOperationDifferences(GenericChangedEntry<IOperation> changedEntry, List<string> logs)
        {
            var diff = Comparer.Compare(changedEntry.OldEntry, changedEntry.NewEntry);

            switch (changedEntry.OldEntry)
            {
                case Shipment shipment:
                    logs.AddRange(GetShipmentChanges(shipment, changedEntry.NewEntry as Shipment));
                    diff.AddRange(Comparer.Compare(shipment, changedEntry.NewEntry as Shipment));
                    break;
                case PaymentIn payment:
                    logs.AddRange(GetPaymentChanges(payment, changedEntry.NewEntry as PaymentIn));
                    diff.AddRange(Comparer.Compare(payment, changedEntry.NewEntry as PaymentIn));
                    break;
                case CustomerOrder order:
                    logs.AddRange(GetCustomerOrderChanges(order, changedEntry.NewEntry as CustomerOrder));
                    diff.AddRange(Comparer.Compare(order, changedEntry.NewEntry as CustomerOrder));
                    break;
                case Capture capture:
                    diff.AddRange(Comparer.Compare(capture, changedEntry.NewEntry as Capture));
                    break;
                case Refund refund:
                    diff.AddRange(Comparer.Compare(refund, changedEntry.NewEntry as Refund));
                    break;
            }

            return diff;
        }

        protected virtual IEnumerable<string> GetCustomerOrderChanges(CustomerOrder originalOrder, CustomerOrder modifiedOrder)
        {
            var result = new List<string>();

            if (originalOrder.EmployeeId != modifiedOrder.EmployeeId)
            {
                var employeeName = "none";
                if (!string.IsNullOrEmpty(modifiedOrder.EmployeeId))
                {
                    var employee = _memberService.GetByIdAsync(modifiedOrder.EmployeeId).GetAwaiter().GetResult() as Employee;
                    employeeName = employee?.FullName ?? employeeName;
                }
                result.Add($"Order employee was changed to '{employeeName}'");
            }

            result.AddRange(GetAddressChanges(originalOrder, originalOrder.Addresses, modifiedOrder.Addresses));

            return result;
        }

        protected virtual IEnumerable<string> GetShipmentChanges(Shipment originalShipment, Shipment modifiedShipment)
        {
            return GetAddressChanges(originalShipment, [originalShipment.DeliveryAddress], [modifiedShipment.DeliveryAddress]);
        }

        protected virtual IEnumerable<string> GetPaymentChanges(PaymentIn payment, PaymentIn modifiedPayment)
        {
            return GetAddressChanges(payment, [payment.BillingAddress], [modifiedPayment.BillingAddress]);
        }

        protected virtual IEnumerable<string> GetAddressChanges(IOperation operation, IEnumerable<Address> originalAddress, IEnumerable<Address> modifiedAddress)
        {
            var result = new List<string>();

            var modifiedAddressList = modifiedAddress?.Where(x => x != null).ToList() ?? [];
            var originalAddressList = originalAddress?.Where(x => x != null).ToList() ?? [];

            modifiedAddressList.CompareTo(originalAddressList, EqualityComparer<Address>.Default, (state, _, target) =>
            {
                switch (state)
                {
                    case EntryState.Added:
                        result.Add($"The address '{StringifyAddress(target)}' for {operation.OperationType} {operation.Number} added");
                        break;
                    case EntryState.Deleted:
                        result.Add($"The address '{StringifyAddress(target)}' for {operation.OperationType} {operation.Number} deleted");
                        break;
                }
            });

            return result;
        }

        protected virtual string StringifyAddress(Address address)
        {
            return address is null
                ? string.Empty
                : string.Join(", ", address.GetAllProperties().Select(p => p.GetValue(address)).Where(x => x != null));
        }

        protected virtual OperationLog GetLogRecord(IOperation operation, string template, EntryState operationType = EntryState.Modified)
        {
            var result = AbstractTypeFactory<OperationLog>.TryCreateInstance();

            result.ObjectId = operation.Id;
            result.ObjectType = operation.GetType().Name;
            result.OperationType = operationType;
            result.Detail = template;

            return result;
        }
    }

    internal sealed class DifferenceComparer : EqualityComparer<Difference>
    {
        public override bool Equals(Difference x, Difference y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                   Equals(x.OldValue, y.OldValue) &&
                   Equals(x.NewValue, y.NewValue);
        }

        public override int GetHashCode(Difference obj)
        {
            return HashCode.Combine(obj.Name, obj.OldValue, obj.NewValue);
        }
    }
}
