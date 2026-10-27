// Maps an order/payment/shipment status to a VcStatus variant (dot color of the design's StatusPill).
type Variant = "info" | "warning" | "danger" | "success" | "primary";

const buckets: Record<Variant, string[]> = {
  primary: ["new", "created", "open"],
  warning: ["pending", "waiting", "processing", "in progress", "inprogress", "onhold", "on hold", "partially paid", "authorized", "requested"],
  success: ["completed", "complete", "approved", "published", "paid", "shipped", "active", "accepted", "delivered", "ready", "succeeded", "success"],
  danger: ["cancelled", "canceled", "rejected", "declined", "failed", "error", "voided", "expired", "refunded"],
  info: [],
};

export function statusVariant(status?: string): Variant {
  const value = (status ?? "").trim().toLowerCase();
  return (Object.keys(buckets) as Variant[]).find((k) => buckets[k].includes(value)) ?? "info";
}
