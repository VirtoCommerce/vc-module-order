<template>
  <VcBlade
    :title="isNew ? $t('ORDERS.PAGES.FILTER.NEW_TITLE') : filter.name"
    :toolbar-items="bladeToolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="name"
          rules="required"
          :model-value="filter.name"
        >
          <VcInput
            v-model="filter.name"
            :label="$t('ORDERS.PAGES.FILTER.NAME')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcSelect
          v-model="filter.statuses"
          :label="$t('ORDERS.PAGES.LIST.FILTERS.STATUS')"
          :placeholder="$t('ORDERS.PAGES.FILTER.ALL')"
          :options="statuses"
          multiple
        />
        <VcSelect
          v-model="filter.storeIds"
          :label="$t('ORDERS.PAGES.LIST.FILTERS.STORE')"
          :placeholder="$t('ORDERS.PAGES.FILTER.ALL')"
          :options="stores"
          option-value="id"
          option-label="name"
          multiple
        />
        <VcSelect
          v-model="filter.customerId"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.CUSTOMER')"
          :placeholder="$t('ORDERS.PAGES.FILTER.ALL')"
          :options="searchContacts"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcSelect
          v-model="filter.organizationId"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.ORGANIZATION')"
          :placeholder="$t('ORDERS.PAGES.FILTER.ALL')"
          :options="searchOrganizations"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="filter.startDate"
              type="date"
              :label="$t('ORDERS.PAGES.FILTER.FROM')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="filter.endDate"
              type="date"
              :label="$t('ORDERS.PAGES.FILTER.TO')"
            />
          </VcCol>
        </VcRow>
        <VcSelect
          v-model="filter.employeeId"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.EMPLOYEE')"
          :placeholder="$t('ORDERS.PAGES.FILTER.ALL')"
          :options="searchEmployees"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcSwitch
          v-model="filter.withPrototypes"
          :label="$t('ORDERS.PAGES.LIST.FILTERS.WITH_PROTOTYPES')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// filter-detail: a named, saved order filter (stored in this browser, like the AngularJS $localStorage filters)
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcSwitch } from "@vc-shell/framework/ui";

import { Field } from "vee-validate";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useSavedFilters, type SavedOrderFilter } from "../composables/useSavedFilters";
import { useSettingValues } from "../composables/useSettingValues";
import { clone } from "../utils/operations";
import { useStores } from "../composables/useStores";
import { getMember, searchEmployees, searchMembers } from "../api";

defineBlade({ name: "OrderFilterDetails" });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ filterId?: string }>();
const { filters, currentId } = useSavedFilters();
const statuses = useSettingValues("Order.Status");
const stores = useStores();
const searchContacts = searchMembers("Contact");
const searchOrganizations = searchMembers("Organization");

const stored = (id?: string) => filters.value.find((f) => f.id === id);
const existing = () => stored(filter.value?.id ?? options.value?.filterId);
const isNew = computed(() => !existing());
// dates are edited as the date picker's local-midnight Dates and stored as their ISO strings (as the AngularJS
// datepicker did), so a saved day reopens as the same day in any time zone
const toEdit = (f: SavedOrderFilter) => ({
  ...clone(f),
  startDate: f.startDate ? new Date(f.startDate) : undefined,
  endDate: f.endDate ? new Date(f.endDate) : undefined,
});
const initial = stored(options.value?.filterId);
const filter = ref<SavedOrderFilter>(
  initial ? toEdit(initial) : { id: String(Date.now()), name: t("ORDERS.PAGES.FILTER.UNNAMED") },
);
const form = useBladeForm({ data: filter });
form.setBaseline();

const isoDate = (v?: unknown) => (v ? new Date(v as string).toISOString() : undefined);

async function apply() {
  const value = { ...clone(filter.value), startDate: isoDate(filter.value.startDate), endDate: isoDate(filter.value.endDate) };
  // orders keep the customer's user account ids, not the contact id
  value.customerIds = value.customerId
    ? ((await getMember(value.customerId).catch(() => undefined))?.securityAccounts ?? []).map((a) => a.id!).filter(Boolean)
    : undefined;
  const index = filters.value.findIndex((f) => f.id === value.id);
  if (index >= 0) filters.value.splice(index, 1, value);
  else filters.value.unshift(value);
  currentId.value = value.id;
  form.setBaseline();
  await callParent("reloadFirstPage");
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("ORDERS.PAGES.FILTER.APPLY"),
    icon: "lucide-filter",
    disabled: computed(() => !form.formMeta.value.valid || !filter.value.name),
    clickHandler: apply,
  },
  {
    id: "reset",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => {
      if (existing()) filter.value = toEdit(existing()!);
      form.setBaseline();
    },
  },
  {
    id: "delete",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: computed(() => !isNew.value),
    async clickHandler() {
      filters.value = filters.value.filter((f) => f.id !== filter.value.id);
      if (currentId.value === filter.value.id) currentId.value = undefined;
      form.setBaseline();
      await callParent("reloadFirstPage");
      await closeSelf();
    },
  },
]);
</script>
