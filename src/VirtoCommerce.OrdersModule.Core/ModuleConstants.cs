using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Core
{
    [ExcludeFromCodeCoverage]
    public class ModuleConstants
    {
        public const string OrderIndexDocumentTypeConstant = nameof(CustomerOrder);

        public static string OrderIndexDocumentType { get; } = OrderIndexDocumentTypeConstant;

        public const string PurchasedProductDocumentPrefix = "__purchased_by_user";

        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "order:read";
                public const string Create = "order:create";
                public const string Update = "order:update";
                public const string Access = "order:access";
                public const string Delete = "order:delete";
                public const string ReadPrices = "order:read_prices";
                public const string UpdateShipments = "order:update_shipments";
                public const string CapturePayment = "order:capture_payment";
                public const string RefundPayment = "order:refund";
                public const string ViewDashboardStatistics = "order:dashboardstatistics:view";

                public static string[] AllPermissions { get; } = {
                    Read,
                    Create,
                    Update,
                    Access,
                    Delete,
                    ReadPrices,
                    UpdateShipments,
                    CapturePayment,
                    RefundPayment,
                    ViewDashboardStatistics,
                };
            }
        }

        public static class CustomerOrderStatus
        {
            public const string New = "New";
            public const string NotPayed = "Not payed";
            public const string Pending = "Pending";
            public const string Processing = "Processing";
            public const string ReadyToSend = "Ready to send";
            public const string Cancelled = "Cancelled";
            public const string PartiallySent = "Partially sent";
            public const string Completed = "Completed";
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor OrderStatus = new SettingDescriptor
                {
                    Name = "Order.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    IsLocalizable = true,
                    AllowedValues = new object[]
                    {
                        CustomerOrderStatus.New,
                        CustomerOrderStatus.NotPayed,
                        CustomerOrderStatus.Pending,
                        CustomerOrderStatus.Processing,
                        CustomerOrderStatus.ReadyToSend,
                        CustomerOrderStatus.Cancelled,
                        CustomerOrderStatus.PartiallySent,
                        CustomerOrderStatus.Completed,
                    }
                };

                public static SettingDescriptor OrderInitialStatus { get; } = new()
                {
                    Name = "Order.InitialStatus",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    DefaultValue = CustomerOrderStatus.New,
                };

                public static SettingDescriptor OrderInitialProcessingStatus { get; } = new()
                {
                    Name = "Order.InitialProcessingStatus",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    DefaultValue = CustomerOrderStatus.Processing,
                };

                public static SettingDescriptor OrderLineItemStatuses { get; } = new()
                {
                    Name = "OrderLineItem.Statuses",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    IsLocalizable = true,
                    AllowedValues = new object[] { "Pending", "InProgress", "Shipped", "Delivered", "Cancelled" },
                };

                public static SettingDescriptor OrderLineItemInitialStatus { get; } = new()
                {
                    Name = "OrderLineItem.InitialStatus",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                };

                public static SettingDescriptor ShipmentStatus = new SettingDescriptor
                {
                    Name = "Shipment.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    IsLocalizable = true,
                    DefaultValue = "New",
                    AllowedValues = new object[] { "New", "PickPack", "Cancelled", "ReadyToSend", "Sent" }
                };

                public static SettingDescriptor PaymentInStatus = new SettingDescriptor
                {
                    Name = "PaymentIn.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    IsLocalizable = true,
                    DefaultValue = "New",
                    AllowedValues = new object[] { "New", "Pending", "Authorized", "Paid", "PartiallyRefunded", "Refunded", "Voided", "Custom", "Cancelled" }
                };

                public static SettingDescriptor RefundStatus { get; } = new SettingDescriptor
                {
                    Name = "Refund.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    IsLocalizable = true,
                    DefaultValue = "Pending",
                    AllowedValues = new object[] { "Pending", "Rejected", "Processed" }
                };

                public static SettingDescriptor OrderCustomerOrderNewNumberTemplate = new SettingDescriptor
                {
                    Name = "Order.CustomerOrderNewNumberTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|Orders",
                    DefaultValue = "CO{0:yyMMdd}-{1:D5}"
                };

                public static SettingDescriptor OrderShipmentNewNumberTemplate = new SettingDescriptor
                {
                    Name = "Order.ShipmentNewNumberTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|Orders",
                    DefaultValue = "SH{0:yyMMdd}-{1:D5}"
                };

                public static SettingDescriptor OrderPaymentInNewNumberTemplate = new SettingDescriptor
                {
                    Name = "Order.PaymentInNewNumberTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|Orders",
                    DefaultValue = "PI{0:yyMMdd}-{1:D5}"
                };

                public static SettingDescriptor RefundNewNumberTemplate { get; } = new SettingDescriptor
                {
                    Name = "Order.RefundNewNumberTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|Orders",
                    DefaultValue = "RE{0:yyMMdd}-{1:D5}"
                };

                public static SettingDescriptor CaptureNewNumberTemplate { get; } = new SettingDescriptor
                {
                    Name = "Order.CaptureNewNumberTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|Orders",
                    DefaultValue = "CA{0:yyMMdd}-{1:D5}"
                };

                public static SettingDescriptor SendOrderNotifications = new SettingDescriptor
                {
                    Name = "Order.SendOrderNotifications",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor OrderAdjustInventory = new SettingDescriptor
                {
                    Name = "Order.AdjustInventory",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor LogOrderChanges { get; } = new SettingDescriptor
                {
                    Name = "Order.LogOrderChanges",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "Order.Search.EventBasedIndexation.Enable",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor CustomerOrderIndexationDate { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.CustomerOrder",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime),
                };

                public static SettingDescriptor CustomerOrderValidation { get; } = new SettingDescriptor
                {
                    Name = "Order.Validation.Enable",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor OrderPaidAndOrderSentNotifications { get; } = new SettingDescriptor
                {
                    Name = "Order.OrderPaidAndOrderSentNotifications.Enable",
                    GroupName = "Orders|Notification",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor PaymentShipmentStatusChangedNotifications { get; } = new SettingDescriptor
                {
                    Name = "Order.PaymentShipmentStatusChangedNotifications.Enable",
                    GroupName = "Orders|Notification",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static SettingDescriptor PurchasedProductIndexation { get; } = new SettingDescriptor
                {
                    Name = "Order.PurchasedProductIndexation.Enable",
                    GroupName = "Orders|Products",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static SettingDescriptor EventBasedPurchasedProductIndexation { get; } = new SettingDescriptor
                {
                    Name = "Order.EventBasedPurchasedProductIndexation.Enable",
                    GroupName = "Orders|Products",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };

                public static SettingDescriptor PurchasedProductStoreFilter { get; } = new SettingDescriptor
                {
                    Name = "Order.PurchasedProductStoreFilter.Enable",
                    GroupName = "Orders|Products",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                    IsPublic = true,
                };

                public static SettingDescriptor MaxOrderDocumentCount { get; } = new SettingDescriptor
                {
                    Name = "Order.MaxOrderDocumentCount",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = 20,
                };

                public static SettingDescriptor DashboardStatisticsEnabled { get; } = new SettingDescriptor
                {
                    Name = "Order.DashboardStatistics.Enable",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor DashboardStatisticsRangeMonths { get; } = new SettingDescriptor
                {
                    Name = "Order.DashboardStatistics.RangeMonths",
                    GroupName = "Orders|General",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 12,
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return OrderStatus;
                        yield return OrderInitialStatus;
                        yield return OrderInitialProcessingStatus;
                        yield return OrderLineItemStatuses;
                        yield return OrderLineItemInitialStatus;
                        yield return ShipmentStatus;
                        yield return PaymentInStatus;
                        yield return RefundStatus;
                        yield return OrderCustomerOrderNewNumberTemplate;
                        yield return OrderShipmentNewNumberTemplate;
                        yield return OrderPaymentInNewNumberTemplate;
                        yield return RefundNewNumberTemplate;
                        yield return SendOrderNotifications;
                        yield return OrderAdjustInventory;
                        yield return LogOrderChanges;
                        yield return CustomerOrderValidation;
                        yield return OrderPaidAndOrderSentNotifications;
                        yield return PaymentShipmentStatusChangedNotifications;
                        yield return PurchasedProductIndexation;
                        yield return EventBasedPurchasedProductIndexation;
                        yield return PurchasedProductStoreFilter;
                        yield return MaxOrderDocumentCount;
                        yield return DashboardStatisticsEnabled;
                        yield return DashboardStatisticsRangeMonths;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> StoreLevelSettings
            {
                get
                {
                    yield return General.OrderCustomerOrderNewNumberTemplate;
                    yield return General.OrderPaymentInNewNumberTemplate;
                    yield return General.OrderShipmentNewNumberTemplate;
                    yield return General.RefundNewNumberTemplate;
                    yield return General.PurchasedProductStoreFilter;
                }
            }

            public static IEnumerable<SettingDescriptor> IndexationSettings
            {
                get
                {
                    yield return General.EventBasedIndexation;
                    yield return General.CustomerOrderIndexationDate;
                }
            }
        }
    }
}
