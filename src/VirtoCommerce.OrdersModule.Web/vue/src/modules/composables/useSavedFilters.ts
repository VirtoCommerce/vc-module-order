import { ref, watch } from "vue";

// Saved order filters of the AngularJS list ($localStorage.orderSearchFilters), shared by the list and filter blades.
export interface SavedOrderFilter {
  id: string;
  name: string;
  statuses?: string[];
  storeIds?: string[];
  customerId?: string;
  customerIds?: string[];
  organizationId?: string;
  employeeId?: string;
  // ISO strings when stored; Dates while edited in the filter blade
  startDate?: string | Date;
  endDate?: string | Date;
  withPrototypes?: boolean;
  predefined?: boolean;
}

const KEY = "vc-orders.savedFilters";
const CURRENT_KEY = "vc-orders.savedFilterId";

function read<T>(key: string, fallback: T): T {
  try {
    return JSON.parse(localStorage.getItem(key) ?? "") as T;
  } catch {
    return fallback;
  }
}

const filters = ref<SavedOrderFilter[]>(read(KEY, []));
const currentId = ref<string | undefined>(read<string | undefined>(CURRENT_KEY, undefined));
watch(filters, (v) => localStorage.setItem(KEY, JSON.stringify(v)), { deep: true });
watch(currentId, (v) => (v ? localStorage.setItem(CURRENT_KEY, JSON.stringify(v)) : localStorage.removeItem(CURRENT_KEY)));

// today / last day / last week / last month / last year, recomputed on every use (buildPredefinedFilters)
export function predefinedFilters(): SavedOrderFilter[] {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const shift = (days: number) => new Date(today.getFullYear(), today.getMonth(), today.getDate() + days);
  const weekStart = shift(-today.getDay() - 6);
  const weekEnd = shift(-today.getDay() + 1);
  const iso = (d: Date) => d.toISOString();
  return [
    { id: "today", name: "TODAY", startDate: iso(today) },
    { id: "last-day", name: "LAST_DAY", startDate: iso(shift(-1)), endDate: iso(today) },
    { id: "last-week", name: "LAST_WEEK", startDate: iso(weekStart), endDate: iso(weekEnd) },
    {
      id: "last-month",
      name: "LAST_MONTH",
      startDate: iso(new Date(today.getFullYear(), today.getMonth() - 1, 1)),
      endDate: iso(new Date(today.getFullYear(), today.getMonth(), 1)),
    },
    {
      id: "last-year",
      name: "LAST_YEAR",
      startDate: iso(new Date(today.getFullYear() - 1, 0, 1)),
      endDate: iso(new Date(today.getFullYear(), 0, 1)),
    },
  ].map((f) => ({ ...f, predefined: true }));
}

export function useSavedFilters() {
  const find = (id?: string) => (id ? [...predefinedFilters(), ...filters.value].find((f) => f.id === id) : undefined);
  return { filters, currentId, find };
}
