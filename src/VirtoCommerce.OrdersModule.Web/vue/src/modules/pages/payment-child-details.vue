<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <div class="orders-state">
          <VcStatus
            v-if="entity.status"
            :variant="statusVariant(entity.isCancelled ? 'Cancelled' : entity.status)"
            >{{ entity.status }}</VcStatus
          >
          <VcHint
            v-if="entity.isCancelled && entity.cancelReason"
            >{{ entity.cancelReason }}</VcHint>
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
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcField
              :label="$t('ORDERS.PAGES.OPERATION.CREATED')"
              :model-value="entity.createdDate"
              type="date"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcField
              :label="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.TRANSACTION')"
              :model-value="entity.transactionId"
              copyable
            />
          </VcCol>
          <VcCol>
            <VcField
              :label="$t('ORDERS.PAGES.PAYMENT_ACTION.OUTER_ID')"
              :model-value="entity.outerId"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInputCurrency
              v-if="withPrices"
              v-model="entity.amount"
              :label="$t('ORDERS.PAGES.PAYMENT_ACTION.AMOUNT')"
              :option="entity.currency"
              :suffix="entity.currency"
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
            />
          </VcCol>
        </VcRow>
        <VcSwitch
          v-if="kind === 'captures'"
          :model-value="entity.closeTransaction"
          :label="$t('ORDERS.PAGES.PAYMENT_ACTION.CLOSE_TRANSACTION')"
          disabled
        />
        <VcRow
          v-else
          class="tw-gap-4"
        >
          <VcCol>
            <VcSelect
              v-model="entity.status"
              :label="$t('ORDERS.PAGES.OPERATION.STATUS')"
              :options="statuses"
              :clearable="false"
              :disabled="!!entity.outerId || entity.status === 'Processed'"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="entity.reasonCode"
              :label="$t('ORDERS.PAGES.PAYMENT_ACTION.REASON')"
              :options="refundReasons"
            />
          </VcCol>
        </VcRow>
        <VcTextarea
          v-model="entity.comment"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.COMMENT')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// capture-detail / refund-detail of the AngularJS module
import { useBlade, useBladeWidgets, usePermissions } from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcHint,
  VcInput,
  VcInputCurrency,
  VcRow,
  VcSelect,
  VcStatus,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useOperationEditor } from "../composables/useOperationEditor";
import { useSettingValues } from "../composables/useSettingValues";
import { useDynamicPropertyCount } from "../composables/useDynamicProps";
import { useRefundReasons } from "../utils/refundReasons";
import { statusVariant } from "../utils/status";
import { searchAnyMember } from "../api";

defineBlade({ name: "OrderPaymentChildDetails" });

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();
const { kind, order, entity, withPrices, loading, toolbar, operationType } = useOperationEditor();
const statuses = useSettingValues("Refund.Status");
const refundReasons = useRefundReasons();

const title = computed(() =>
  t(kind === "captures" ? "ORDERS.PAGES.PAYMENT_ACTION.CAPTURE_TITLE" : "ORDERS.PAGES.PAYMENT_ACTION.REFUND_TITLE", {
    number: entity.value.number,
  }),
);

const dynamicPropertyCount = useDynamicPropertyCount(computed(() => entity.value.objectType));

useBladeWidgets([
  {
    id: "PaymentChildProperties",
    icon: "lucide-list-plus",
    title: "ORDERS.WIDGETS.PROPERTIES",
    badge: dynamicPropertyCount,
    isVisible: hasAccess("platform:dynamic_properties:read"),
    onClick: () => openBlade({ name: "OrderDynamicProperties", options: { orderId: order.item.value.id } }),
  },
  {
    id: "PaymentChildChanges",
    icon: "lucide-history",
    title: "ORDERS.WIDGETS.CHANGES",
    onClick: () => openBlade({ name: "OrderChanges", param: entity.value.id, options: { objectType: operationType } }),
  },
]);
</script>
