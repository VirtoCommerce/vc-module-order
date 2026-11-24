import { usePermissions, type GlobalFilterConfig, type SearchBarField } from "@vc-shell/framework";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { CustomerOrderSearchCriteria } from "../../api_client/virtocommerce.orders";
import { useSettingValues } from "./useSettingValues";
import { STORE_READ, useStores } from "./useStores";

/** A search bar value as a list: a multi-select sends an array, a single select one value */
export function filterList(value: unknown): string[] | undefined {
  if (!Array.isArray(value)) return value ? [String(value)] : undefined;
  return value.length ? value.map(String) : undefined;
}

/**
 * A date range's day as the instant a search takes: the start of the first day or the end of the last, in the user's
 * time zone. The bar sends whole days ("2026-11-10"), which `new Date` would read as UTC midnight.
 */
export function filterDay(value: unknown, endOfDay: boolean): Date | undefined {
  if (typeof value !== "string" || !value) return undefined;
  const ymd = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!ymd) return new Date(value);
  const [y, m, d] = ymd.slice(1).map(Number);
  return endOfDay ? new Date(y, m - 1, d, 23, 59, 59, 999) : new Date(y, m - 1, d);
}

/** The search bar's filters, which VcDataTable passes to the bar as they are but still types as the older filters */
export const asTableFilters = (fields: SearchBarField[]) => fields as unknown as GlobalFilterConfig[];

/**
 * The orders search bar's filters: Status (several), Created (a date range) and, when `withStores` and the user may
 * read stores (VCST-6547), Store (several). `criteria` holds what the bar picked, as search criteria.
 */
export function useOrderFilters(withStores: boolean) {
  const { t } = useI18n({ useScope: "global" });
  const { hasAccess } = usePermissions();
  const statuses = useSettingValues("Order.Status");
  const storesShown = withStores && hasAccess(STORE_READ);
  const stores = storesShown ? useStores() : ref([]);
  const criteria = ref<CustomerOrderSearchCriteria>({});

  const globalFilters = computed(() =>
    asTableFilters([
      {
        id: "statuses",
        label: t("ORDERS.PAGES.LIST.FILTERS.STATUS"),
        filter: { options: statuses.value.map((s) => ({ value: s.id, label: s.title })), multiple: true },
      },
      {
        id: "created",
        label: t("ORDERS.PAGES.LIST.FILTERS.CREATED"),
        filter: { range: ["startDate", "endDate"] as [string, string] },
      },
      ...(storesShown
        ? [
            {
              id: "storeIds",
              label: t("ORDERS.PAGES.LIST.FILTERS.STORE"),
              filter: { options: stores.value.map((s) => ({ value: s.id!, label: s.name ?? s.id! })), multiple: true },
            },
          ]
        : []),
    ]),
  );

  /** Takes the filters of the bar's `filter` event */
  function applyFilters(filters: Record<string, unknown>) {
    const next: CustomerOrderSearchCriteria = {
      statuses: filterList(filters.statuses),
      storeIds: filterList(filters.storeIds),
      startDate: filterDay(filters.startDate, false),
      endDate: filterDay(filters.endDate, true),
    };
    criteria.value = Object.fromEntries(Object.entries(next).filter(([, v]) => v !== undefined));
  }

  return { globalFilters, criteria, applyFilters };
}
