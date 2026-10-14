<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <div
          v-if="!isNew"
          class="orders-state"
        >
          <VcStatus
            v-if="entity.status"
            :variant="statusVariant(entity.isCancelled ? 'Cancelled' : entity.status)"
            >{{ entity.status }}</VcStatus
          >
          <VcHint
            v-if="entity.isCancelled && entity.cancelReason"
            >{{ entity.cancelReason }}</VcHint>
        </div>
        <Field
          v-if="isNew"
          v-slot="{ errorMessage, handleChange, errors }"
          name="shipmentMethodCode"
          rules="required"
          :model-value="entity.shipmentMethodCode"
        >
          <VcSelect
            v-model="entity.shipmentMethodCode"
            :label="$t('ORDERS.PAGES.SHIPMENT.METHOD')"
            :options="methods"
            option-value="code"
            option-label="name"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="(v) => (handleChange(v), onMethodChanged(v))"
          />
        </Field>
        <div
          v-else
          class="orders-brand"
        >
          <VcImage
            v-if="entity.shippingMethod?.logoUrl"
            :src="entity.shippingMethod.logoUrl"
            size="m"
            background="contain"
          />
          <div>
            <div class="orders-brand__name">{{ entity.shippingMethod?.name ?? entity.shipmentMethodCode }}</div>
            <div
              v-if="entity.shippingMethod?.optionName"
              class="orders-brand__descr"
            >
              {{ entity.shippingMethod.optionName }}
            </div>
          </div>
        </div>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="entity.fulfillmentCenterId"
              :label="$t('ORDERS.PAGES.SHIPMENT.FULFILLMENT_CENTER')"
              :options="centers"
              option-value="id"
              option-label="name"
              :disabled="locked"
              @update:model-value="(v) => (entity.fulfillmentCenterName = centers.find((c) => c.id === v)?.name)"
            />
          </VcCol>
          <VcCol>
            <VcField
              v-if="!isNew"
              :label="$t('ORDERS.PAGES.SHIPMENT.METHOD_OPTION')"
              :model-value="entity.shipmentMethodOption"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="number"
              rules="required"
              :model-value="entity.number"
            >
              <VcInput
                v-model="entity.number"
                :label="$t('ORDERS.PAGES.SHIPMENT.NUMBER')"
                required
                :disabled="locked"
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcInput
              v-if="isNew"
              v-model="entity.createdDate"
              type="datetime-local"
              :label="$t('ORDERS.PAGES.OPERATION.CREATED')"
            />
            <VcField
              v-else
              :label="$t('ORDERS.PAGES.OPERATION.CREATED')"
              :model-value="entity.createdDate"
              type="date"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="entity.status"
              :label="$t('ORDERS.PAGES.OPERATION.STATUS')"
              :options="statuses"
              :clearable="false"
              :disabled="locked"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="entity.employeeId"
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.EMPLOYEE')"
              :options="searchEmployees"
              option-value="id"
              option-label="name"
              searchable
              :disabled="locked"
              @update:model-value="async (v) => (entity.employeeName = await employeeName(v))"
            />
          </VcCol>
        </VcRow>
        <VcRow
          v-if="withPrices"
          class="tw-gap-4"
        >
          <VcCol>
            <VcInputCurrency
              v-model="entity.price"
              :label="$t('ORDERS.PAGES.OPERATION.PRICE')"
              :option="entity.currency"
              :suffix="entity.currency"
              :disabled="locked"
            />
          </VcCol>
          <VcCol>
            <VcInputCurrency
              v-model="entity.priceWithTax"
              :label="$t('ORDERS.PAGES.OPERATION.PRICE_WITH_TAX')"
              :option="entity.currency"
              :suffix="entity.currency"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="entity.trackingNumber"
              :label="$t('ORDERS.PAGES.SHIPMENT.TRACKING_NUMBER')"
              :disabled="locked"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="entity.deliveryDate"
              type="datetime-local"
              :label="$t('ORDERS.PAGES.SHIPMENT.DELIVERY_DATE')"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="entity.trackingUrl"
              :label="$t('ORDERS.PAGES.SHIPMENT.TRACKING_URL')"
              :disabled="locked"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="entity.vendorId"
              :label="$t('ORDERS.PAGES.OPERATION.VENDOR')"
              :options="searchAnyMember"
              option-value="id"
              option-label="name"
              searchable
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcSwitch
          v-model="entity.isApproved"
          :label="$t('ORDERS.PAGES.OPERATION.APPROVED')"
          :disabled="locked"
        />
        <VcTextarea
          v-model="entity.comment"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.COMMENT')"
          :disabled="locked"
        />
        <OperationTotals
          v-if="withPrices && !isNew"
          :operation="entity"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeWidgets, usePermissions } from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcHint,
  VcImage,
  VcInput,
  VcInputCurrency,
  VcRow,
  VcSelect,
  VcStatus,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOperationEditor } from "../composables/useOperationEditor";
import { useSettingValues } from "../composables/useSettingValues";
import { useDynamicPropertyCount } from "../composables/useDynamicProps";
import OperationTotals from "../components/OperationTotals.vue";
import { statusVariant } from "../utils/status";
import {
  employeeName,
  searchAnyMember,
  searchEmployees,
  searchFulfillmentCenters,
  searchShippingMethods,
  type NamedEntity,
} from "../api";

defineBlade({ name: "OrderShipmentDetails" });

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();
const { order, entity, isNew, locked, withPrices, loading, toolbar } = useOperationEditor("shipments");
const statuses = useSettingValues("Shipment.Status");

const title = computed(() =>
  isNew.value ? t("ORDERS.PAGES.SHIPMENT.NEW_TITLE") : t("ORDERS.PAGES.SHIPMENT.TITLE", { number: entity.value.number }),
);

const methods = ref<NamedEntity[]>([]);
searchShippingMethods(order.item.value.storeId).then((r) => (methods.value = r.results ?? []));
const centers = ref<NamedEntity[]>([]);
searchFulfillmentCenters().then((r) => (centers.value = r.results ?? []));

function onMethodChanged(code: unknown) {
  entity.value.shippingMethod = methods.value.find((m) => m.code === code);
}

const dynamicPropertyCount = useDynamicPropertyCount(computed(() => entity.value.objectType));
const openChild = (name: string, options: Record<string, unknown> = {}) =>
  openBlade({ name, options: { orderId: order.item.value.id, ...options } });

useBladeWidgets([
  {
    id: "ShipmentItems",
    icon: "lucide-package",
    title: "ORDERS.WIDGETS.SHIPMENT_ITEMS",
    badge: computed(() => entity.value.items?.length ?? 0),
    onClick: () => openChild("OrderShipmentItems"),
  },
  {
    id: "ShipmentAddress",
    icon: "lucide-map-pin",
    title: "ORDERS.WIDGETS.DELIVERY_ADDRESS",
    badge: computed(() => (entity.value.deliveryAddress ? 1 : 0)),
    onClick: () => openChild("OrderAddressDetails", { field: "deliveryAddress", addressType: "Shipping" }),
  },
  {
    id: "ShipmentDiscounts",
    icon: "lucide-percent",
    title: "ORDERS.WIDGETS.DISCOUNTS",
    badge: computed(() => entity.value.discounts?.length ?? 0),
    onClick: () => openChild("OrderDiscounts"),
  },
  {
    id: "ShipmentProperties",
    icon: "lucide-list-plus",
    title: "ORDERS.WIDGETS.PROPERTIES",
    badge: dynamicPropertyCount,
    isVisible: hasAccess("platform:dynamic_properties:read"),
    onClick: () => openChild("OrderDynamicProperties"),
  },
  {
    id: "ShipmentChanges",
    icon: "lucide-history",
    title: "ORDERS.WIDGETS.CHANGES",
    isVisible: computed(() => !isNew.value),
    onClick: () => openBlade({ name: "OrderChanges", param: entity.value.id, options: { objectType: "Shipment" } }),
  },
]);
</script>

<style lang="scss">
/* payment / shipment method header of the AngularJS operation blades */
.orders-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  &__name {
    font-size: 15px;
    font-weight: 600;
    color: var(--color-neutral-900);
  }
  &__descr {
    font-size: 13px;
    color: var(--color-neutral-500);
  }
}
</style>
