using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly ConcurrentDictionary<string, List<string>> _auditablePropertiesListByTypeDict = new();

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
                var operationLogs = new List<OperationLog>();

                foreach (var changedEntry in message.ChangedEntries)
                {
                    if (changedEntry.EntryState == EntryState.Modified)
                    {
                        var originalOperations = changedEntry.OldEntry.GetFlatObjectsListWithInterface<IOperation>().Distinct().ToList();
                        var modifiedOperations = changedEntry.NewEntry.GetFlatObjectsListWithInterface<IOperation>().Distinct().ToList();

                        modifiedOperations.CompareTo(originalOperations, EqualityComparer<IOperation>.Default,
                                                             (state, modified, original) => operationLogs.AddRange(GetChangedEntryOperationLogs(new GenericChangedEntry<IOperation>(modified, original, state))));
                    }
                    else if (changedEntry.EntryState == EntryState.Added || changedEntry.EntryState == EntryState.Deleted)
                    {
                        operationLogs.AddRange(GetChangedEntryOperationLogs(new GenericChangedEntry<IOperation>(changedEntry.NewEntry, changedEntry.OldEntry, changedEntry.EntryState)));
                    }
                }

                if (!operationLogs.IsNullOrEmpty())
                {
                    BackgroundJob.Enqueue(() => TryToLogChangesBackgroundJob(operationLogs.ToArray()));
                }
            }
        }

        // (!) Do not make this method async, it causes improper user recorded into the log! It happens because the user stored in the current thread. If the thread switched, the user info will lost.
        public void TryToLogChangesBackgroundJob(OperationLog[] operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
        }

        protected virtual IEnumerable<OperationLog> GetChangedEntryOperationLogs(GenericChangedEntry<IOperation> changedEntry)
        {
            var result = new List<OperationLog>();

            //TDB: Rework to make more extensible
            if (changedEntry.EntryState == EntryState.Modified)
            {
                var logs = new List<string>();

                var diff = GetOperationDifferences(changedEntry, logs);

                var auditableProperties = GetAuditableProperties(changedEntry);

                if (auditableProperties.Count != 0)
                {
                    var observedDifferences = diff.Join(auditableProperties, x => x.Name.ToLowerInvariant(), x => x.ToLowerInvariant(), (x, y) => x).ToArray();
                    foreach (var difference in observedDifferences.Distinct(new DifferenceComparer()))
                    {
                        logs.Add($"The {changedEntry.OldEntry.OperationType} {changedEntry.NewEntry.Number} property '{difference.Name}' changed from '{difference.OldValue}' to '{difference.NewValue}'");
                    }
                }

                result.AddRange(logs.Select(x => GetLogRecord(changedEntry.NewEntry, x)));
            }
            else if (changedEntry.EntryState == EntryState.Deleted)
            {
                var record = GetLogRecord(changedEntry.NewEntry, $"The {changedEntry.NewEntry.OperationType} {changedEntry.NewEntry.Number} deleted");
                record.OperationType = EntryState.Deleted;
                result.Add(record);
            }
            else if (changedEntry.EntryState == EntryState.Added)
            {
                var record = GetLogRecord(changedEntry.NewEntry, $"The new {changedEntry.NewEntry.OperationType} {changedEntry.NewEntry.Number} added");
                record.OperationType = EntryState.Added;
                result.Add(record);
            }

            return result;
        }

        protected List<string> GetAuditableProperties(GenericChangedEntry<IOperation> changedEntry)
        {
            var type = changedEntry.OldEntry.GetType();
            if (!_auditablePropertiesListByTypeDict.TryGetValue(type.Name, out var auditableProperties))
            {
                auditableProperties = type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(AuditableAttribute)))
                                                          .Select(x => x.Name)
                                                          .ToList();
                _auditablePropertiesListByTypeDict[type.Name] = auditableProperties;
            }

            return auditableProperties;
        }

        protected virtual IList<Difference> GetOperationDifferences(GenericChangedEntry<IOperation> changedEntry, List<string> logs)
        {
            var diff = Comparer.Compare(changedEntry.OldEntry, changedEntry.NewEntry);

            if (changedEntry.OldEntry is Shipment shipment)
            {
                logs.AddRange(GetShipmentChanges(shipment, changedEntry.NewEntry as Shipment));
                diff.AddRange(Comparer.Compare(shipment, changedEntry.NewEntry as Shipment));
            }
            else if (changedEntry.OldEntry is PaymentIn payment)
            {
                logs.AddRange(GetPaymentChanges(payment, changedEntry.NewEntry as PaymentIn));
                diff.AddRange(Comparer.Compare(payment, changedEntry.NewEntry as PaymentIn));
            }
            else if (changedEntry.OldEntry is CustomerOrder order)
            {
                logs.AddRange(GetCustomerOrderChanges(order, changedEntry.NewEntry as CustomerOrder));
                diff.AddRange(Comparer.Compare(order, changedEntry.NewEntry as CustomerOrder));
            }
            else if (changedEntry.OldEntry is Capture capture)
            {
                diff.AddRange(Comparer.Compare(capture, changedEntry.NewEntry as Capture));
            }
            else if (changedEntry.OldEntry is Refund refund)
            {
                diff.AddRange(Comparer.Compare(refund, changedEntry.NewEntry as Refund));
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
                    employeeName = employee != null ? employee.FullName : employeeName;
                }
                result.Add($"Order employee was changed to '{employeeName}'");
            }
            result.AddRange(GetAddressChanges(originalOrder, originalOrder.Addresses, modifiedOrder.Addresses));
            return result.ToArray();
        }

        protected virtual IEnumerable<string> GetShipmentChanges(Shipment originalShipment, Shipment modifiedShipment)
        {
            var retVal = new List<string>();
            retVal.AddRange(GetAddressChanges(originalShipment, new[] { originalShipment.DeliveryAddress }, new[] { modifiedShipment.DeliveryAddress }));
            return retVal.ToArray();
        }

        protected virtual IEnumerable<string> GetPaymentChanges(PaymentIn payment, PaymentIn modifiedPayment)
        {
            var result = new List<string>();
            result.AddRange(GetAddressChanges(payment, new[] { payment.BillingAddress }, new[] { modifiedPayment.BillingAddress }));
            return result;
        }

        protected virtual IEnumerable<string> GetAddressChanges(IOperation operation, IEnumerable<Address> originalAddress, IEnumerable<Address> modifiedAddress)
        {
            var result = new List<string>();
            modifiedAddress.Where(x => x != null).ToList().CompareTo(originalAddress.Where(x => x != null).ToList(), EqualityComparer<Address>.Default,
                                      (state, source, target) =>
                                      {
                                          if (state == EntryState.Added)
                                          {
                                              result.Add($"The address '{StringifyAddress(target)}' for {operation.OperationType} {operation.Number} added");
                                          }
                                          else if (state == EntryState.Deleted)
                                          {
                                              result.Add($"The address '{StringifyAddress(target)}' for {operation.OperationType} {operation.Number} deleted");
                                          }
                                      });
            return result;
        }

        protected virtual string StringifyAddress(Address address)
        {
            var result = "";
            if (address != null)
            {
                return string.Join(", ", typeof(Address).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                   .OrderBy(p => p.Name)
                                   .Select(p => p.GetValue(address))
                                   .Where(x => x != null));
            }
            return result;
        }

        protected virtual OperationLog GetLogRecord(IOperation operation, string template)
        {
            var result = AbstractTypeFactory<OperationLog>.TryCreateInstance();

            result.ObjectId = operation.Id;
            result.ObjectType = operation.GetType().Name;
            result.OperationType = EntryState.Modified;
            result.Detail = template;

            return result;
        }
    }

    internal class DifferenceComparer : EqualityComparer<Difference>
    {
        public override bool Equals(Difference x, Difference y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public override int GetHashCode(Difference obj)
        {
            var result = string.Join(":", obj.Name, obj.NewValue, obj.OldValue);
            return result.GetHashCode();
        }
    }
}
