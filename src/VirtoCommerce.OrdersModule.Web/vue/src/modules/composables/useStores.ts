import { ref } from "vue";
import { searchStores, type NamedEntity } from "../api";

/** The stores, as select / filter options. */
export function useStores() {
  const stores = ref<NamedEntity[]>([]);
  searchStores().then((r) => (stores.value = r.results ?? []));
  return stores;
}
