import { onUnmounted, type ComputedRef, type Ref } from "vue";
import type { CustomerOrder } from "../../api_client/virtocommerce.orders";

// Like the AngularJS blades, every child blade (items, payments, shipments, addresses, properties...) edits the
// order loaded by OrdersDetails in memory; the order blade's Save persists everything with one PUT.
// The order blade registers its live state here; children look it up by order id (blade options hold only ids,
// because the order object itself is replaced on every recalculate/reload).
export interface OrderHandle {
  item: Ref<CustomerOrder>;
  locked: ComputedRef<boolean>;
  recalculate: () => Promise<void>;
  reload: () => Promise<void>;
}

const handles = new Map<string, OrderHandle>();

export function provideOrder(id: string, handle: OrderHandle) {
  handles.set(id, handle);
  onUnmounted(() => {
    if (handles.get(id) === handle) handles.delete(id);
  });
}

export function useOrder(id?: string): OrderHandle {
  const handle = id ? handles.get(id) : undefined;
  if (!handle) throw new Error(`[orders] order ${id} is not open`);
  return handle;
}
