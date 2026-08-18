using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that adjusts reserved stock for a changed order.
    /// </summary>
    /// <remarks>
    /// Deliberately a projection of the order rather than the order itself: a payload is serialized without type
    /// information, so a whole <see cref="CustomerOrder"/> cannot be read back on the worker - it reaches
    /// <see cref="PaymentIn.PaymentMethod"/>, <see cref="Shipment.ShippingMethod"/> or
    /// <see cref="OrderOperation.ChildrenOperations"/> and fails on an abstract or interface type. This carries the fields
    /// <see cref="Handlers.AdjustInventoryOrderChangedEventHandler.ProcessInventoryChanges"/> actually reads, and
    /// nothing else - which also keeps the job store small, since the order graph would otherwise be stored twice.
    /// </remarks>
    public class AdjustInventoryJobPayload
    {
        /// <summary>Id of the changed order.</summary>
        public string OrderId { get; set; }

        /// <summary>Store the changed order belongs to, used to resolve the fulfillment centers.</summary>
        public string StoreId { get; set; }

        /// <summary>How the order itself changed.</summary>
        public EntryState EntryState { get; set; }

        /// <summary>Order status after the change.</summary>
        public string NewStatus { get; set; }

        /// <summary>Order status before the change.</summary>
        public string OldStatus { get; set; }

        /// <summary>Ordered line items after the change.</summary>
        public IList<AdjustInventoryJobLineItem> NewItems { get; set; } = [];

        /// <summary>Ordered line items before the change.</summary>
        public IList<AdjustInventoryJobLineItem> OldItems { get; set; } = [];

        public static AdjustInventoryJobPayload FromChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = AbstractTypeFactory<AdjustInventoryJobPayload>.TryCreateInstance();

            result.OrderId = changedEntry.NewEntry.Id;
            result.StoreId = changedEntry.NewEntry.StoreId;
            result.EntryState = changedEntry.EntryState;
            result.NewStatus = changedEntry.NewEntry.Status;
            result.OldStatus = changedEntry.OldEntry?.Status;
            result.NewItems = ToJobLineItems(changedEntry.NewEntry.Items);
            result.OldItems = ToJobLineItems(changedEntry.OldEntry?.Items);

            return result;
        }

        /// <summary>
        /// Restores the entry the handler works with. Everything the projection does not carry stays unset, so the
        /// orders are only ever valid as input for the inventory adjustment.
        /// </summary>
        public virtual GenericChangedEntry<CustomerOrder> ToChangedEntry()
        {
            return new GenericChangedEntry<CustomerOrder>(
                ToOrder(NewStatus, NewItems),
                ToOrder(OldStatus, OldItems),
                EntryState);
        }

        protected virtual CustomerOrder ToOrder(string status, IList<AdjustInventoryJobLineItem> items)
        {
            var order = AbstractTypeFactory<CustomerOrder>.TryCreateInstance();

            order.Id = OrderId;
            order.StoreId = StoreId;
            order.Status = status;
            order.Items = items?.Select(x => x.ToLineItem()).ToList() ?? [];

            return order;
        }

        protected static IList<AdjustInventoryJobLineItem> ToJobLineItems(ICollection<LineItem> items)
        {
            return items?.Select(AdjustInventoryJobLineItem.FromLineItem).ToList() ?? [];
        }
    }
}
