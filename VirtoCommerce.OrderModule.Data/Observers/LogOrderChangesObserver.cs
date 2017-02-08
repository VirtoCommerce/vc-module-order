using System;
using System.Collections.Generic;
using System.Linq;
using AutoCompare;
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
        private static string[] _observedProperties;
        static LogOrderChangesObserver()
        {
            var operationPropNames = ReflectionUtility.GetPropertyNames<IOperation>(x => x.Status, x => x.Comment, x => x.Currency, x => x.Number, x => x.IsApproved);
            var orderPropNames = ReflectionUtility.GetPropertyNames<CustomerOrder>(x => x.DiscountAmount, x => x.Total, x => x.Fee, x => x.Number, x => x.TaxPercentRate, x => x.TaxTotal, x => x.TaxType);
            var shipmentPropNames = ReflectionUtility.GetPropertyNames<Shipment>(x => x.DiscountAmount, x => x.Total, x => x.Fee, x => x.Number, x => x.TaxPercentRate, x => x.TaxTotal, x => x.TaxType, x => x.Height, x => x.Length, x => x.MeasureUnit, x => x.Price, x => x.ShipmentMethodCode, x => x.ShipmentMethodOption, x => x.Weight, x => x.WeightUnit, x => x.Width);
            var paymentPropNames = ReflectionUtility.GetPropertyNames<PaymentIn>(x => x.DiscountAmount, x => x.Total, x => x.Number, x => x.TaxPercentRate, x => x.TaxTotal, x => x.TaxType, x => x.OuterId, x => x.AuthorizedDate, x => x.CapturedDate, x => x.Price, x => x.GatewayCode, x => x.IncomingDate, x => x.Purpose, x => x.VoidedDate);

            _observedProperties = operationPropNames.Concat(orderPropNames).Concat(shipmentPropNames).Concat(paymentPropNames).Distinct().ToArray();
        }
        public LogOrderChangesObserver(IChangeLogService changeLogService, IMemberService memberService, string[] observedProperties)
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
                                                      (state, modified, original) => operationLogs.AddRange(GetOperationLogs(state, original, modified)));                                    

                _changeLogService.SaveChanges(operationLogs.ToArray());
            }
        }
        #endregion


        private IEnumerable<OperationLog> GetOperationLogs(EntryState changeState, IOperation original, IOperation modified)
        {
            var retVal = new List<string>();
            if (changeState == EntryState.Modified)
            {
                var diff = Comparer.Compare(original, modified);
               
                if (original is Shipment)
                {
                    retVal.AddRange(GetShipmentChanges(original as Shipment, modified as Shipment));
                    diff.AddRange(Comparer.Compare<Shipment>(original as Shipment, modified as Shipment));
                }
                if (original is PaymentIn)
                {
                    retVal.AddRange(GetPaymentChanges(original as PaymentIn, modified as PaymentIn));
                    diff.AddRange(Comparer.Compare<PaymentIn>(original as PaymentIn, modified as PaymentIn));
                }
                if (original is CustomerOrder)
                {
                    retVal.AddRange(GetCustomerOrderChanges(original as CustomerOrder, modified as CustomerOrder));
                    diff.AddRange(Comparer.Compare<CustomerOrder>(original as CustomerOrder, modified as CustomerOrder));
                }
                foreach (var change in diff.Join(_observedProperties, x => x.Name.ToLowerInvariant(), x => x.ToLowerInvariant(), (x, y) => x).Distinct())
                {
                    retVal.Add(string.Format(OrderResources.OperationPropertyChanged, original.OperationType, modified.Number, change.Name, change.OldValue, change.NewValue));
                }
            }
            else if (changeState == EntryState.Deleted)
            {
                retVal.Add(string.Format(OrderResources.OperationDeleted, modified.OperationType, modified.Number));
            }
            else if (changeState == EntryState.Added)
            {
                retVal.Add(string.Format(OrderResources.OperationAdded, modified.OperationType, modified.Number));
            }
            return retVal.Select(x=> GetLogRecord(modified, x));
        }

        private string[] GetCustomerOrderChanges(CustomerOrder originalOrder, CustomerOrder modifiedOrder)
        {
            var retVal = new List<string>();
            if (originalOrder.EmployeeId != modifiedOrder.EmployeeId)
            {
                var employeeName = "none";
                if (!string.IsNullOrEmpty(modifiedOrder.EmployeeId))
                {
                    var employee = _memberService.GetByIds(new[] { modifiedOrder.EmployeeId }).OfType<Employee>().FirstOrDefault();
                    employeeName = employee != null ? employee.FullName : employeeName;
                }
                retVal.Add(string.Format(OrderResources.OrderEmployeeChanged, employeeName));
            }          
            retVal.AddRange(GetAddressChanges(originalOrder, originalOrder.Addresses, modifiedOrder.Addresses));       
            return retVal.ToArray();
        }

        private string[] GetShipmentChanges(Shipment originalShipment, Shipment modifiedShipment)
        {
            var retVal = new List<string>();
            retVal.AddRange(GetAddressChanges(originalShipment, new Address[] { originalShipment.DeliveryAddress }, new Address[] { modifiedShipment.DeliveryAddress }));
            return retVal.ToArray();
        }

        private string[] GetPaymentChanges(PaymentIn payment, PaymentIn modifiedPayment)
        {
            var retVal = new List<string>();      
            retVal.AddRange(GetAddressChanges(payment, new Address[] { payment.BillingAddress }, new Address[] { modifiedPayment.BillingAddress }));
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
                ObjectType = operation.GetType().Name,
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
