import { useBlade } from "@vc-shell/framework";

/** Opens orders from a dashboard widget: the order opens over the orders list, as it does from the list itself. */
export function useOrderNavigation() {
  const { openBlade } = useBlade();
  const openList = () => openBlade({ name: "OrdersList", isWorkspace: true });
  async function openOrder(id?: string) {
    await openList();
    if (id) await openBlade({ name: "OrdersDetails", param: id });
  }
  return { openList, openOrder };
}
