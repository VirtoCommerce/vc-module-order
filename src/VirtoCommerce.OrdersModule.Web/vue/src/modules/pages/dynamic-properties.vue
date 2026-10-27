<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.PROPERTIES')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <div
          v-if="languages.length > 1 && properties.some((p) => p.multilanguage)"
          class="orders-properties__language"
        >
          <VcLabel>{{ $t("ORDERS.PAGES.PROPERTIES.LANGUAGE") }}: {{ currentLanguage }}</VcLabel>
          <VcLanguageSelector
            v-model="currentLanguage"
            :options="languages.map((l) => ({ value: l, label: l }))"
          />
        </div>
        <div
          v-if="!loading && !properties.length"
          class="orders-empty"
        >
          {{ $t("ORDERS.PAGES.PROPERTIES.EMPTY") }}
        </div>
        <VcDynamicProperty
          v-for="property in properties"
          :key="property.id + currentLanguage"
          :property="property"
          :model-value="valueOf(property)"
          :options-getter="loadDictionaries"
          :required="!!property.required"
          :multivalue="property.multivalue"
          :multilanguage="property.multilanguage"
          :current-language="currentLanguage"
          :value-type="property.valueType ?? 'ShortText'"
          :dictionary="property.dictionary"
          :name="property.name ?? ''"
          :display-names="property.displayNames"
          :disabled="locked"
          @update:model-value="(ev: unknown) => onChange(property, ev)"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// The platform's dynamic properties widget/blade (propertyValue-list) for the order, its operations and line items,
// edited with the framework's VcDynamicProperty. OK writes the values back into the entity (saved with the order).
import {
  DynamicPropertiesClient,
  useApiClient,
  useBlade,
  useBladeForm,
  useDynamicProperties,
  type DynamicObjectProperty,
  type DynamicProperty,
  type DynamicPropertyObjectValue,
  type IBaseProperty,
  type IBasePropertyValue,
  type IBladeToolbar,
} from "@vc-shell/framework";
import {
  VcBlade,
  VcContainer,
  VcDynamicProperty,
  VcForm,
  VcLabel,
  VcLanguageSelector,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getSettingValues } from "../api";
import { useOrder } from "../composables/useOrder";
import { useDynamicPropertyDefinitions } from "../composables/useDynamicProps";
import type { AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderDynamicProperties" });

// a platform dynamic property in the shape VcDynamicProperty / useDynamicProperties work with (catalog-like)
type EditedProperty = IBaseProperty & {
  required?: boolean;
  displayNames?: { name?: string; languageCode?: string }[];
  definition: DynamicProperty;
};

const { t, locale } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ orderId: string }>();
const { getApiClient } = useApiClient(DynamicPropertiesClient);
const loadDefinitions = useDynamicPropertyDefinitions();
const order = useOrder(options.value?.orderId);
const locked = order.locked;

const owner = ref<AnyOperation>();
const properties = ref<EditedProperty[]>([]);
const languages = ref<string[]>([]);
const currentLanguage = ref<string>(locale.value);
const loading = ref(true);
const form = useBladeForm({ data: properties });

// platform value types VcDynamicProperty has no control for: Decimal → number, Html → long text, Image → URL text
const controlType: Record<string, string> = { Decimal: "Number", Html: "LongText", Image: "ShortText" };

const toEdited = (definition: DynamicProperty, values: DynamicPropertyObjectValue[]): EditedProperty => ({
  id: definition.id,
  name: definition.name,
  valueType: controlType[definition.valueType ?? ""] ?? definition.valueType,
  multivalue: definition.isArray,
  multilanguage: definition.isMultilingual,
  dictionary: definition.isDictionary,
  required: definition.isRequired,
  displayNames: definition.displayNames?.map((d) => ({ name: d.name, languageCode: d.locale })),
  values: values.map((v) => ({
    propertyId: definition.id,
    propertyName: definition.name,
    languageCode: v.locale,
    alias: v.value,
    value: v.value,
    valueId: v.valueId,
  })),
  definition,
});

const toValues = (p: EditedProperty): DynamicPropertyObjectValue[] =>
  (p.values ?? [])
    .filter((v) => v.value !== undefined && v.value !== null && v.value !== "")
    .map((v) => ({
      propertyId: p.definition.id,
      propertyName: p.definition.name,
      valueType: p.definition.valueType,
      locale: v.languageCode,
      value: v.value,
      valueId: v.valueId,
    }));

// dictionary items in the shape loadDictionaries expects (alias = name, localized display names); each dictionary is
// fetched once per blade: the editors ask again on every search keystroke and language switch, and filter here
const dictionaries = new Map<string, ReturnType<DynamicPropertiesClient["getAllDictionaryItems"]>>();
function dictionary(id: string) {
  let items = dictionaries.get(id);
  if (!items) {
    items = getApiClient().then((c) => c.getAllDictionaryItems(id));
    items.catch(() => dictionaries.delete(id)); // a failed lookup is retried
    dictionaries.set(id, items);
  }
  return items;
}
const { getPropertyValue, setPropertyValue, loadDictionaries } = useDynamicProperties({
  searchDictionary: async (criteria) => {
    const items = (await Promise.all((criteria.propertyIds ?? []).map(dictionary))).flat();
    const keyword = criteria.keyword?.toLowerCase();
    return items
      .filter((i) => !keyword || i.name?.toLowerCase().includes(keyword))
      .map((i) => ({
        id: i.id,
        propertyId: i.propertyId,
        alias: i.name,
        value: i.name,
        localizedValues: i.displayNames?.map((d) => ({ languageCode: d.locale, value: d.name })),
      }));
  },
});

// VcMultivalue (free, non-dictionary arrays) labels its chips by `title`; the values carry `value`
const isFreeMultivalue = (p: EditedProperty) => !!p.multivalue && !p.dictionary;
function valueOf(property: EditedProperty) {
  const value = getPropertyValue(property, currentLanguage.value);
  return isFreeMultivalue(property) && Array.isArray(value)
    ? value.map((v) => ({ ...(v as object), title: (v as IBasePropertyValue).value }))
    : value;
}

function onChange(property: EditedProperty, ev: unknown) {
  const change = (ev && typeof ev === "object" && "value" in ev ? { ...ev } : { value: ev }) as { value: unknown };
  if (isFreeMultivalue(property) && Array.isArray(change.value))
    change.value = change.value.map(({ title, ...v }: { title?: unknown; value?: unknown }) => ({
      ...v,
      value: v.value ?? title,
    }));
  setPropertyValue({ property, ...change } as Parameters<typeof setPropertyValue>[0]);
  // a new array so the blade form's deep watcher sees nested value changes
  properties.value = [...properties.value];
}

onMounted(async () => {
  try {
    owner.value = await callParent<AnyOperation>("getEntity");
    if (!owner.value?.objectType) return;
    const definitions = [...(await loadDefinitions(owner.value.objectType, true))].sort(
      (a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0) || (a.name ?? "").localeCompare(b.name ?? ""),
    );
    if (definitions.some((d) => d.isMultilingual)) {
      languages.value = await getSettingValues("VirtoCommerce.Core.General.Languages");
      if (!languages.value.includes(currentLanguage.value) && languages.value.length)
        currentLanguage.value = languages.value[0];
    }
    const current = (owner.value.dynamicProperties ?? []) as DynamicObjectProperty[];
    properties.value = definitions.map((d) => toEdited(d, current.find((x) => x.name === d.name)?.values ?? []));
  } finally {
    loading.value = false;
    form.setBaseline();
  }
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("ORDERS.PAGES.OPERATION.TOOLBAR.APPLY"),
    icon: "lucide-check",
    permissions: "order:update",
    disabled: computed(() => locked.value || !form.canSave.value),
    async clickHandler() {
      // keep values of properties this blade doesn't know (e.g. deleted definitions), replace the rest
      const known = new Set(properties.value.map((p) => p.name));
      const others = ((owner.value!.dynamicProperties ?? []) as DynamicObjectProperty[]).filter((x) => !known.has(x.name));
      owner.value!.dynamicProperties = [
        ...others,
        ...properties.value.map(({ definition: d, ...p }) => ({
          name: d.name,
          id: d.id,
          objectType: d.objectType,
          valueType: d.valueType,
          isArray: d.isArray,
          isDictionary: d.isDictionary,
          isMultilingual: d.isMultilingual,
          isRequired: d.isRequired,
          values: toValues({ ...p, definition: d }),
        })),
      ];
      form.setBaseline();
      await closeSelf();
    },
  },
  {
    id: "reset",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => {
      const current = (owner.value?.dynamicProperties ?? []) as DynamicObjectProperty[];
      properties.value = properties.value.map((p) =>
        toEdited(p.definition, current.find((x) => x.name === p.definition.name)?.values ?? []),
      );
      form.setBaseline();
    },
  },
]);
</script>

<style lang="scss">
.orders-empty {
  padding: 24px 0;
  text-align: center;
  font-size: 14px;
  color: var(--color-neutral-500);
}

.orders-properties__language {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
}
</style>
