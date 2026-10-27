<template>
  <VcBlade
    :title="isNew ? $t('ORDERS.PAGES.ADDRESSES.NEW_TITLE') : $t('ORDERS.PAGES.ADDRESSES.TITLE')"
    :toolbar-items="bladeToolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <VcTextarea
          v-model="address.description"
          :label="$t('ORDERS.PAGES.ADDRESSES.DESCRIPTION')"
          maxlength="128"
        />
        <VcSelect
          v-model="address.addressType"
          :label="$t('ORDERS.PAGES.ADDRESSES.TYPE')"
          :options="addressTypes"
          :clearable="false"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="address.firstName"
              :label="$t('ORDERS.PAGES.ADDRESSES.FIRST_NAME')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.middleName"
              :label="$t('ORDERS.PAGES.ADDRESSES.MIDDLE_NAME')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.lastName"
              :label="$t('ORDERS.PAGES.ADDRESSES.LAST_NAME')"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="countryCode"
              rules="required"
              :model-value="address.countryCode"
            >
              <VcSelect
                v-model="address.countryCode"
                :label="$t('ORDERS.PAGES.ADDRESSES.COUNTRY')"
                :options="countries"
                option-value="id"
                option-label="name"
                searchable
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="(v: unknown) => onCountryPicked(v, handleChange)"
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="address.regionName"
              :label="$t('ORDERS.PAGES.ADDRESSES.REGION')"
              :options="regions"
              option-value="name"
              option-label="name"
              searchable
              @update:model-value="onRegionPicked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="city"
              rules="required"
              :model-value="address.city"
            >
              <VcInput
                v-model="address.city"
                :label="$t('ORDERS.PAGES.ADDRESSES.CITY')"
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="postalCode"
              rules="required"
              :model-value="address.postalCode"
            >
              <VcInput
                v-model="address.postalCode"
                :label="$t('ORDERS.PAGES.ADDRESSES.POSTAL_CODE')"
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
        </VcRow>
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="line1"
          rules="required"
          :model-value="address.line1"
        >
          <VcInput
            v-model="address.line1"
            :label="$t('ORDERS.PAGES.ADDRESSES.LINE1')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcInput
          v-model="address.line2"
          :label="$t('ORDERS.PAGES.ADDRESSES.LINE2')"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="address.email"
              type="email"
              :label="$t('ORDERS.PAGES.ADDRESSES.EMAIL')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.phone"
              :label="$t('ORDERS.PAGES.ADDRESSES.PHONE')"
            />
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// address-details (order addresses) and the payment billing / shipment delivery address blades.
// Works on its parent's entity: `field` names a single address property, otherwise `index` points into `addresses`.
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcTextarea } from "@vc-shell/framework/ui";

import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import { getCountries, getRegions, type NamedEntity } from "../api";
import { clone, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderAddressDetails" });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{
  orderId: string;
  field?: string;
  index?: number;
  addressType?: string;
}>();
const { showConfirmation } = usePopup();
const order = useOrder(options.value?.orderId);

const addressTypes = ["Billing", "Shipping", "BillingAndShipping"].map((x) => ({ id: x, title: x }));
const owner = ref<AnyOperation>();
const address = ref<AnyOperation>({});
const isNew = ref(true);
const form = useBladeForm({ data: address });
const blank = () => ({ addressType: options.value?.addressType ?? "Shipping" });

const countries = ref<NamedEntity[]>([]);
const regions = ref<NamedEntity[]>([]);
getCountries().then((x) => (countries.value = x));

function current(): AnyOperation | undefined {
  const o = owner.value;
  if (!o) return undefined;
  if (options.value?.field) return o[options.value.field] ?? undefined;
  return options.value?.index === undefined ? undefined : o.addresses?.[options.value.index];
}

async function loadRegions(code?: string) {
  regions.value = code ? await getRegions(code).catch(() => []) : [];
}

function onCountryPicked(code: unknown, handleChange: (v: unknown) => void) {
  handleChange(code);
  address.value.countryName = countries.value.find((c) => c.id === code)?.name;
  address.value.regionName = undefined;
  address.value.regionId = undefined;
  loadRegions(code as string);
}

function onRegionPicked(name: unknown) {
  address.value.regionId = regions.value.find((r) => r.name === name)?.id;
}

onMounted(async () => {
  owner.value = await callParent<AnyOperation>("getEntity");
  const existing = current();
  isNew.value = !existing;
  address.value = clone(existing ?? blank());
  form.setBaseline();
  await loadRegions(address.value.countryCode);
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("ORDERS.PAGES.OPERATION.TOOLBAR.APPLY"),
    icon: "lucide-check",
    permissions: "order:update",
    disabled: computed(() => order.locked.value || !form.canSave.value),
    async clickHandler() {
      const o = owner.value!;
      const value = clone(address.value);
      if (options.value?.field) o[options.value.field] = value;
      else if (isNew.value) {
        o.addresses ??= [];
        o.addresses.push(value);
      } else o.addresses.splice(options.value!.index!, 1, value);
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
      address.value = clone(current() ?? blank());
      form.setBaseline();
    },
  },
  {
    id: "delete",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "order:update",
    isVisible: computed(() => !isNew.value),
    disabled: order.locked,
    async clickHandler() {
      if (!(await showConfirmation(t("ORDERS.ALERTS.DELETE_ADDRESS_CONFIRMATION")))) return;
      const o = owner.value!;
      if (options.value?.field) o[options.value.field] = null;
      else o.addresses.splice(options.value!.index!, 1);
      form.setBaseline();
      await closeSelf();
    },
  },
]);
</script>
