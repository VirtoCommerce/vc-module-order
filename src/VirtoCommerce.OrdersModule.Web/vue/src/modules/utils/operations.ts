import { toRaw } from "vue";
import { useI18n } from "vue-i18n";
import type { CustomerOrder } from "../../api_client/virtocommerce.orders";

export type OperationKind = "payments" | "shipments" | "captures" | "refunds";

/** Per kind of operation: its list title, amount field, details blade and operationType. */
export const KINDS: Record<OperationKind, { title: string; amountField: string; blade: string; operationType: string }> = {
  payments: { title: "ORDERS.WIDGETS.PAYMENTS", amountField: "sum", blade: "OrderPaymentDetails", operationType: "PaymentIn" },
  shipments: { title: "ORDERS.WIDGETS.SHIPMENTS", amountField: "total", blade: "OrderShipmentDetails", operationType: "Shipment" },
  captures: { title: "ORDERS.WIDGETS.CAPTURES", amountField: "amount", blade: "OrderPaymentChildDetails", operationType: "Capture" },
  refunds: { title: "ORDERS.WIDGETS.REFUNDS", amountField: "amount", blade: "OrderPaymentChildDetails", operationType: "Refund" },
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type AnyOperation = Record<string, any> & { id?: string; number?: string; status?: string };

// knownOperations.isLocked from order.js
export function isLocked(operationType: string | undefined, entity?: AnyOperation): boolean {
  if (!entity) return false;
  const cancelled = entity.isCancelled || entity.cancelledState === "Completed";
  switch (operationType) {
    case "CustomerOrder":
      return entity.status === "Completed" || cancelled;
    case "PaymentIn":
      return entity.status === "Paid" || entity.cancelledState === "Requested" || cancelled;
    case "Shipment":
      return entity.status === "Send" || entity.cancelledState === "Requested" || cancelled;
    default:
      return false;
  }
}

// "Cancel document" availability from operation-detail.js
export function canCancel(entity?: AnyOperation): boolean {
  if (!entity) return false;
  switch (entity.operationType) {
    case "CustomerOrder":
      return !entity.isCancelled && entity.status !== "Completed";
    case "PaymentIn":
      return !(entity.isCancelled || entity.cancelledState === "Completed" || entity.cancelledState === "Requested");
    case "Refund":
      return !entity.isCancelled && entity.status !== "Processed" && !entity.outerId;
    default:
      return !entity.isCancelled;
  }
}

// applies a confirmed cancellation the way operation-detail.js does
export function applyCancel(entity: AnyOperation, reason?: string) {
  switch (entity.operationType) {
    case "PaymentIn":
      entity.cancelledState = "Requested";
      break;
    case "CustomerOrder":
      (entity.inPayments ?? []).forEach((p: AnyOperation) => (p.cancelledState = "Requested"));
      entity.isCancelled = true;
      entity.status = "Cancelled";
      break;
    default:
      entity.isCancelled = true;
      entity.status = "Cancelled";
  }
  entity.cancelReason = reason?.trim() ? reason : null;
  entity.cancelledDate = new Date();
}

export function collection(order: CustomerOrder, kind: OperationKind, paymentId?: string): AnyOperation[] {
  if (kind === "payments") {
    order.inPayments ??= [];
    return order.inPayments as AnyOperation[];
  }
  if (kind === "shipments") {
    order.shipments ??= [];
    return order.shipments as AnyOperation[];
  }
  const payment = (order.inPayments ?? []).find((p) => p.id === paymentId) as AnyOperation | undefined;
  if (!payment) return [];
  payment[kind] ??= [];
  return payment[kind] as AnyOperation[];
}

// recalculate returns "0001-01-01" for operations that were never saved (customerOrder-detail clearCreatedDateIfEmpty)
export function clearEmptyDates(order: CustomerOrder) {
  const o = order as AnyOperation;
  const ops = [...(o.inPayments ?? []), ...(o.shipments ?? []), ...(o.childrenOperations ?? [])] as AnyOperation[];
  for (const op of ops) {
    const d = op.createdDate ? new Date(op.createdDate) : undefined;
    if (d && d.getUTCFullYear() < 1900) op.createdDate = undefined;
  }
}

/** Money formatter; amounts are masked when the user may not read prices (legacy showPrice filter). */
export function useMoney() {
  const { n } = useI18n({ useScope: "global" });
  return (value?: number | null, currency?: string, withPrices = true) => {
    if (!withPrices) return "****";
    if (currency) return n(value ?? 0, { style: "currency", currency, minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return n(value ?? 0, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  };
}

/** Deep copy of (possibly reactive, possibly nested-proxy) entity data. */
export function clone<T>(value: T): T {
  const raw = toRaw(value) as unknown;
  if (raw instanceof Date) return new Date(raw) as T;
  if (Array.isArray(raw)) return raw.map(clone) as T;
  if (raw && typeof raw === "object")
    return Object.fromEntries(Object.entries(raw).map(([k, v]) => [k, clone(v)])) as T;
  return raw as T;
}

/**
 * Line item / configuration images are stored relative to the platform's asset storage ("catalog/x.jpg").
 * The AngularJS admin rendered them against its root ("/"), where a file-system asset provider never serves them;
 * the host page lives under /apps/admin-shell/, so they must be made absolute either way.
 * ponytail: assumes the default file-system provider's public path /assets/; read Assets:*:PublicUrl if a blob provider is used.
 */
export const assetUrl = (url?: string) =>
  !url || /^([a-z][a-z0-9+.-]*:|\/)/i.test(url) ? url : `/assets/${url}`;
