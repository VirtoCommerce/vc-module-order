// Orders in the Admin Shell's global search (the header box, Ctrl+K): this module's provider, registered with the shell's
// registry (app.config.globalProperties.$globalSearch; see the shell's docs/global-search.md). A result opens the order
// over the orders list, as the list opens it; "Show all" opens the list filtered by the query.
import { ref, type App } from "vue";
import type { CustomerOrder } from "../api_client/virtocommerce.orders";

/** The part of the shell's global search registry this module uses */
interface GlobalSearch {
  registerProvider(provider: object): void;
  hasPermission(permission: string | string[]): boolean;
  openBlade(options: {
    route: string;
    workspace: string;
    blades?: { name: string; param?: string; options?: Record<string, unknown> }[];
  }): Promise<void>;
}

/** The keyword "Show all" asks the orders list to show; the list takes it (pages/list.vue) */
export const requestedKeyword = ref<string>();

const money = (amount?: number, currency?: string) => {
  if (amount === undefined || amount === null) return undefined;
  try {
    return currency ? new Intl.NumberFormat(undefined, { style: "currency", currency }).format(amount) : String(amount);
  } catch {
    return `${amount} ${currency ?? ""}`.trim();
  }
};

/** Registers the Orders search provider; nothing in a host without global search */
export function registerOrderSearch(app: App) {
  const search = app.config.globalProperties.$globalSearch as GlobalSearch | undefined;
  if (!search) return;

  const openList = () => search.openBlade({ route: "/orders", workspace: "OrdersList" });
  const openOrder = (id: string) =>
    search.openBlade({ route: "/orders", workspace: "OrdersList", blades: [{ name: "OrdersDetails", param: id }] });

  search.registerProvider({
    id: "orders",
    title: "ORDERS.SEARCH.GROUP_TITLE",
    icon: "lucide-file-text",
    order: 100,
    permission: "order:read",
    isAvailable: () => search.hasPermission("order:access"),
    minLength: 2,
    delay: 250,
    aliases: ["order", "orders"],
    // an order number: Orders first
    match: /^CO\d{3,}/i,
    search: async (query: string, context: { take: number; signal: AbortSignal }) => {
      // WithPrices: the Default response group leaves the totals out
      const response = await fetch("/api/order/customerOrders/search", {
        method: "POST",
        signal: context.signal,
        headers: { Accept: "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({ keyword: query, take: context.take, sort: "createdDate:desc", responseGroup: "WithPrices" }),
      });
      if (!response.ok) throw new Error(`Orders search failed: ${response.status}`);
      const result: { totalCount?: number; results?: CustomerOrder[] } = await response.json();
      return {
        total: result.totalCount,
        showAll: async () => {
          await openList();
          requestedKeyword.value = query;
        },
        items: (result.results ?? []).map((order) => ({
          id: order.id,
          title: order.number,
          description: [order.customerName, order.storeName ?? order.storeId].filter(Boolean).join(" · "),
          icon: "lucide-file-text",
          badge: order.status,
          meta: money(order.total, order.currency),
          execute: () => openOrder(order.id!),
        })),
      };
    },
  });
}
