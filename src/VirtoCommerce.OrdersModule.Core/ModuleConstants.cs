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
        public static string OrderIndexDocumentType { get; } = nameof(CustomerOrder);

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

                public static string[] AllPermissions { get; } = { Read, Create, Update, Access, Delete, ReadPrices, UpdateShipments };
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
                    DefaultValue = CustomerOrderStatus.New,
                    AllowedValues = new[]
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

                public static SettingDescriptor ShipmentStatus = new SettingDescriptor
                {
                    Name = "Shipment.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "PickPack", "Cancelled", "ReadyToSend", "Sent" }
                };

                public static SettingDescriptor PaymentInStatus = new SettingDescriptor
                {
                    Name = "PaymentIn.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Pending", "Authorized", "Paid", "PartiallyRefunded", "Refunded", "Voided", "Custom", "Cancelled" }
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
                    DefaultValue = false,
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

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return OrderStatus;
                        yield return ShipmentStatus;
                        yield return PaymentInStatus;
                        yield return OrderCustomerOrderNewNumberTemplate;
                        yield return OrderShipmentNewNumberTemplate;
                        yield return OrderPaymentInNewNumberTemplate;
                        yield return SendOrderNotifications;
                        yield return OrderAdjustInventory;
                        yield return LogOrderChanges;
                        yield return CustomerOrderValidation;
                        yield return OrderPaidAndOrderSentNotifications;
                        yield return PaymentShipmentStatusChangedNotifications;
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
