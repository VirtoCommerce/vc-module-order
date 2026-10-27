<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
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
            v-if="entity.cancelledState && entity.cancelledState !== 'Undefined'"
            >{{ $t("ORDERS.PAGES.OPERATION.CANCELLED_STATE", { state: entity.cancelledState }) }}</VcHint>
          <VcHint
            v-if="entity.isCancelled && entity.cancelReason"
            >{{ entity.cancelReason }}</VcHint>
        </div>
        <Field
          v-if="isNew"
          v-slot="{ errorMessage, handleChange, errors }"
          name="gatewayCode"
          rules="required"
          :model-value="entity.gatewayCode"
        >
          <VcSelect
            v-model="entity.gatewayCode"
            :label="$t('ORDERS.PAGES.PAYMENT.GATEWAY')"
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
            v-if="entity.paymentMethod?.logoUrl"
            :src="entity.paymentMethod.logoUrl"
            size="m"
            background="contain"
          />
          <div>
            <div class="orders-brand__name">{{ entity.paymentMethod?.name ?? entity.gatewayCode }}</div>
            <div
              v-if="entity.paymentMethod?.paymentMethodGroupType"
              class="orders-brand__descr"
            >
              {{ entity.paymentMethod.paymentMethodGroupType }}
            </div>
          </div>
        </div>
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
                :label="$t('ORDERS.PAGES.PAYMENT.NUMBER')"
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
            <VcSelect
              v-model="entity.status"
              :label="$t('ORDERS.PAGES.OPERATION.STATUS')"
              :options="statuses"
              :clearable="false"
              :disabled="locked"
            />
          </VcCol>
          <VcCol>
            <VcInputCurrency
              v-if="withPrices"
              v-model="entity.sum"
              :label="$t('ORDERS.PAGES.PAYMENT.AMOUNT')"
              :option="entity.currency"
              :suffix="entity.currency"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
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
          <VcCol>
            <VcSwitch
              v-model="entity.isApproved"
              :label="$t('ORDERS.PAGES.OPERATION.APPROVED')"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcTextarea
          v-model="entity.purpose"
          :label="$t('ORDERS.PAGES.PAYMENT.PURPOSE')"
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
import { useBlade, useBladeWidgets, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
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
import { searchAnyMember, searchPaymentMethods, type NamedEntity } from "../api";

defineBlade({ name: "OrderPaymentDetails" });

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();
const { order, entity, isNew, locked, withPrices, loading, toolbar } = useOperationEditor("payments");
const statuses = useSettingValues("PaymentIn.Status");

const title = computed(() =>
  isNew.value ? t("ORDERS.PAGES.PAYMENT.NEW_TITLE") : t("ORDERS.PAGES.PAYMENT.TITLE", { number: entity.value.number }),
);

const methods = ref<NamedEntity[]>([]);
searchPaymentMethods(order.item.value.storeId).then((r) => (methods.value = r.results ?? []));
function onMethodChanged(code: unknown) {
  entity.value.paymentMethod = methods.value.find((m) => m.code === code);
}

// payment flow commands of payment-detail.js; they post to the payment gateway, so they act on the saved payment
const openAction = (action: "capture" | "refund") =>
  openBlade({ name: "OrderPaymentAction", options: { action, orderId: order.item.value.id, paymentId: entity.value.id } });

const bladeToolbar = ref<IBladeToolbar[]>([
  ...toolbar,
  {
    id: "capture",
    title: t("ORDERS.PAGES.PAYMENT.CAPTURE"),
    icon: "lucide-hand-coins",
    permissions: "order:capture_payment",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !["Authorized", "Paid"].includes(entity.value.status ?? "")),
    clickHandler: () => openAction("capture"),
  },
  {
    id: "refund",
    title: t("ORDERS.PAGES.PAYMENT.REFUND"),
    icon: "lucide-undo",
    permissions: "order:refund",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !["PartiallyRefunded", "Paid"].includes(entity.value.status ?? "")),
    clickHandler: () => openAction("refund"),
  },
]);

const dynamicPropertyCount = useDynamicPropertyCount(computed(() => entity.value.objectType));
const openChild = (name: string, options: Record<string, unknown> = {}) =>
  openBlade({ name, options: { orderId: order.item.value.id, ...options } });

useBladeWidgets([
  {
    id: "PaymentAddress",
    icon: "lucide-map-pin",
    title: "ORDERS.WIDGETS.BILLING_ADDRESS",
    badge: computed(() => (entity.value.billingAddress ? 1 : 0)),
    onClick: () => openChild("OrderAddressDetails", { field: "billingAddress", addressType: "Billing" }),
  },
  {
    id: "PaymentTransactions",
    icon: "lucide-arrow-left-right",
    title: "ORDERS.WIDGETS.TRANSACTIONS",
    badge: computed(() => entity.value.transactions?.length ?? 0),
    onClick: () => openChild("OrderPaymentTransactions"),
  },
  {
    id: "PaymentCaptures",
    icon: "lucide-hand-coins",
    title: "ORDERS.WIDGETS.CAPTURES",
    badge: computed(() => entity.value.captures?.length ?? 0),
    isVisible: computed(() => !isNew.value),
    onClick: () => openChild("OrderOperations", { kind: "captures", paymentId: entity.value.id }),
  },
  {
    id: "PaymentRefunds",
    icon: "lucide-undo",
    title: "ORDERS.WIDGETS.REFUNDS",
    badge: computed(() => entity.value.refunds?.length ?? 0),
    isVisible: computed(() => !isNew.value),
    onClick: () => openChild("OrderOperations", { kind: "refunds", paymentId: entity.value.id }),
  },
  {
    id: "PaymentDiscounts",
    icon: "lucide-percent",
    title: "ORDERS.WIDGETS.DISCOUNTS",
    badge: computed(() => entity.value.discounts?.length ?? 0),
    onClick: () => openChild("OrderDiscounts"),
  },
  {
    id: "PaymentProperties",
    icon: "lucide-list-plus",
    title: "ORDERS.WIDGETS.PROPERTIES",
    badge: dynamicPropertyCount,
    isVisible: hasAccess("platform:dynamic_properties:read"),
    onClick: () => openChild("OrderDynamicProperties"),
  },
  {
    id: "PaymentChanges",
    icon: "lucide-history",
    title: "ORDERS.WIDGETS.CHANGES",
    isVisible: computed(() => !isNew.value),
    onClick: () =>
      openBlade({ name: "OrderChanges", param: entity.value.id, options: { objectType: "PaymentIn" } }),
  },
]);
</script>
