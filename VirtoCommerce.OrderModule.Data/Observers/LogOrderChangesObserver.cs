using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Resources;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Observers
{
    /// <summary>
    /// Write human readable change log for any subscription change
    /// </summary>
    public sealed class LogOrderChangesObserver : IObserver<OrderChangeEvent>
    {
        private readonly IMemberService _memberService;
        private readonly IChangeLogService _changeLogService;
        public LogOrderChangesObserver(IChangeLogService changeLogService, IMemberService memberService)
        {
            _changeLogService = changeLogService;
            _memberService = memberService;
        }

        #region IObserver<SubscriptionChangeEvent> Members
        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(OrderChangeEvent changeEvent)
        {
            var order = changeEvent.OrigOrder;

            if (changeEvent.ChangeState == Platform.Core.Common.EntryState.Modified)
            {
                var operationLogs = new List<OperationLog>();
                var originalOperations = changeEvent.OrigOrder.GetFlatObjectsListWithInterface<IOperation>().Distinct();
                var modifiedOperations = changeEvent.ModifiedOrder.GetFlatObjectsListWithInterface<IOperation>().Distinct();             
                             
                modifiedOperations.ToList().CompareTo(originalOperations.ToList(), EqualityComparer<IOperation>.Default,
                                                      (state, source, target) => operationLogs.AddRange(GetOperationLogs(state, source, target)));                                    

                _changeLogService.SaveChanges(operationLogs.ToArray());
            }
        }
        #endregion


        private IEnumerable<OperationLog> GetOperationLogs(EntryState changeState, IOperation source, IOperation target)
        {
            var retVal = new List<string>();
            if (changeState == EntryState.Modified)
            {
                if (source.Status != target.Status)
                {
                    retVal.Add(string.Format(OrderResources.OperationStatusChanged, source.OperationType, source.Number, target.Status, source.Status));
                }
                if (source.Comment != target.Comment)
                {
                    retVal.Add(string.Format(OrderResources.OperationCommentChanged, source.OperationType, source.Number));
                }
                if (source is CustomerOrder)
                {
                    retVal.AddRange(GetCustomerOrderChanges(target as CustomerOrder, source as CustomerOrder));
                }
                if (source is Shipment)
                {
                    retVal.AddRange(GetShipmentChanges(target as Shipment, source as Shipment));
                }
            }
            else if (changeState == EntryState.Deleted)
            {
                retVal.Add(string.Format(OrderResources.OperationDeleted, target.OperationType, target.Number));
            }
            else if (changeState == EntryState.Added)
            {
                retVal.Add(string.Format(OrderResources.OperationAdded, target.OperationType, target.Number));
            }
            return retVal.Select(x=> GetLogRecord(target, x));
        }

        private string[] GetCustomerOrderChanges(CustomerOrder originalOrder, CustomerOrder modifiedOrder)
        {
            var retVal = new List<string>();
            if (originalOrder.EmployeeId != modifiedOrder.EmployeeId)
            {
                var employeeName = "none";
                if (string.IsNullOrEmpty(modifiedOrder.EmployeeId))
                {
                    var employee = _memberService.GetByIds(new[] { modifiedOrder.EmployeeId }).OfType<Employee>().FirstOrDefault();
                    employeeName = employee != null ? employee.FullName : employeeName;
                }
                retVal.Add(string.Format(OrderResources.OrderEmployeeChanged, employeeName));
            }
            if(originalOrder.Total != modifiedOrder.Total)
            {
                retVal.Add(string.Format(OrderResources.OperationTotalChanged, originalOrder.OperationType, originalOrder.Number, originalOrder.Total.ToString("0.00"), modifiedOrder.Total.ToString("0.00")));
            }
            retVal.AddRange(GetAddressChanges(originalOrder, originalOrder.Addresses, modifiedOrder.Addresses));
            return retVal.ToArray();
        }

        private string[] GetShipmentChanges(Shipment originalShipment, Shipment modifiedShipment)
        {
            var retVal = new List<string>();
            if (originalShipment.Total != modifiedShipment.Total)
            {
                retVal.Add(string.Format(OrderResources.OperationTotalChanged, modifiedShipment.OperationType, modifiedShipment.Number, originalShipment.Total.ToString("0.00"), modifiedShipment.Total.ToString("0.00")));
            }
            retVal.AddRange(GetAddressChanges(originalShipment, new Address[] { originalShipment.DeliveryAddress }, new Address[] { modifiedShipment.DeliveryAddress }));
            return retVal.ToArray();
        }

        private string[] GetAddressChanges(IOperation operation, IEnumerable<Address> originalAddress, IEnumerable<Address> modifiedAddress)
        {        
            var retVal = new List<string>();
            modifiedAddress.Where(x=>x != null).ToList().CompareTo(originalAddress.Where(x=> x != null).ToList(), new AddressComparer(),
                                      (state, source, target) =>
                                      {
                                          if (state == EntryState.Added)
                                          {
                                              retVal.Add(string.Format(OrderResources.AddressAdded, operation.OperationType, operation.Number, GetAddressString(target)));
                                          }
                                          else if (state == EntryState.Deleted)
                                          {
                                              retVal.Add(string.Format(OrderResources.AddressRemoved, operation.OperationType, operation.Number, GetAddressString(target)));
                                          }
                                      });

            return retVal.ToArray();
        }
        
        private static string GetAddressString(Address address)
        {
            return string.Join(" ", address.FirstName, address.LastName, address.Line1, address.City, address.RegionName, address.PostalCode, address.CountryName);
        } 

        private static OperationLog GetLogRecord(IOperation operation, string template, params object[] parameters)
        {           
            var result = new OperationLog
            {
                ObjectId = operation.Id,
                ObjectType = operation.OperationType,
                OperationType = Platform.Core.Common.EntryState.Modified,
                Detail = string.Format(template, parameters)
            };
            return result;

        }
    }

    internal class AddressComparer : IEqualityComparer<Address>
    {
        #region IEqualityComparer<Discount> Members

        public bool Equals(Address x, Address y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(Address obj)
        {
            var result = String.Join(":", obj.AddressType, obj.Organization, obj.City, obj.CountryCode, obj.CountryName,
                                          obj.Email, obj.FirstName, obj.LastName, obj.Line1, obj.Line2, obj.Phone, obj.PostalCode, obj.RegionId, obj.RegionName);
            return result.GetHashCode();
        }


        #endregion
    }
}
