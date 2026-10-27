import { ref, watch, type Ref } from "vue";
import { DynamicPropertiesClient, useApiClient, type DynamicProperty } from "@vc-shell/framework";

// definitions per object type, shared by the widgets' counters and the properties blade
const cache = new Map<string, Promise<DynamicProperty[]>>();

export function useDynamicPropertyDefinitions() {
  const { getApiClient } = useApiClient(DynamicPropertiesClient);
  return (objectType: string, refresh = false) => {
    if (refresh || !cache.has(objectType)) {
      cache.set(
        objectType,
        getApiClient()
          .then((c) => c.searchDynamicProperties({ objectType, take: 1000 }))
          .then((r) => r.results ?? [])
          .catch((e) => {
            cache.delete(objectType);
            throw e;
          }),
      );
    }
    return cache.get(objectType)!;
  };
}

/** Number of dynamic properties defined for the entity's type (what the legacy widget shows). */
export function useDynamicPropertyCount(objectType: Ref<string | undefined>) {
  const load = useDynamicPropertyDefinitions();
  const count = ref(0);
  watch(
    objectType,
    async (type) => {
      count.value = type ? (await load(type).catch(() => [])).length : 0;
    },
    { immediate: true },
  );
  return count;
}
