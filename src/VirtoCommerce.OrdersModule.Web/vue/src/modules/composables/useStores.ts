import { usePermissions } from "@vc-shell/framework";
import { ref } from "vue";
import { searchStores, type NamedEntity } from "../api";

/** Reading the store list needs the Store module's read permission (api/stores/search answers 403 without it) */
export const STORE_READ = "store:read";

/**
 * The stores, as select / filter options. A user without store:read gets none and no request is sent (VCST-6547):
 * the order shows its own store instead. A failed load leaves the list empty rather than an unhandled error.
 */
export function useStores() {
  const stores = ref<NamedEntity[]>([]);
  const { hasAccess } = usePermissions();
  if (hasAccess(STORE_READ)) {
    searchStores()
      .then((r) => (stores.value = r.results ?? []))
      .catch(() => (stores.value = []));
  }
  return stores;
}
