<template>
  <VcBlade
    :title="title"
    :subtitle="model.transactionId"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <VcInputCurrency
          v-model="model.amount"
          :label="$t('ORDERS.PAGES.PAYMENT_ACTION.AMOUNT')"
          :option="payment?.currency"
          :suffix="payment?.currency"
        />
        <template v-if="isRefund">
          <VcSelect
            v-model="model.reasonCode"
            :label="$t('ORDERS.PAGES.PAYMENT_ACTION.REASON')"
            :options="refundReasons"
            :clearable="false"
            required
          />
          <VcInput
            v-model="model.reasonMessage"
            :label="$t('ORDERS.PAGES.PAYMENT_ACTION.MESSAGE')"
            maxlength="128"
          />
        </template>
        <VcInput
          v-else
          v-model="model.captureDetails"
          :label="$t('ORDERS.PAGES.PAYMENT_ACTION.CAPTURE_DETAILS')"
          maxlength="128"
        />
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="transactionId"
          rules="required"
          :model-value="model.transactionId"
        >
          <VcInput
            v-model="model.transactionId"
            :label="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.TRANSACTION')"
            required
            maxlength="128"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcInput
          v-model="model.outerId"
          :label="$t('ORDERS.PAGES.PAYMENT_ACTION.OUTER_ID')"
          maxlength="128"
        />
        <VcSwitch
          v-if="!isRefund"
          v-model="model.closeTransaction"
          :label="$t('ORDERS.PAGES.PAYMENT_ACTION.CLOSE_TRANSACTION')"
          :tooltip="$t('ORDERS.PAGES.PAYMENT_ACTION.CLOSE_TRANSACTION_HINT')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// capture-add / refund-add: posts the payment-flow request for a saved payment (the gateway decides the outcome)
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcInputCurrency, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import { refundCodes, useRefundReasons } from "../utils/refundReasons";
import { capturePayment, refundPayment } from "../api";
import type { AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderPaymentAction" });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf, setError, clearError } = useBlade<{
  action: "capture" | "refund";
  orderId: string;
  paymentId: string;
}>();
const order = useOrder(options.value?.orderId);
const refundReasons = useRefundReasons();
const isRefund = computed(() => options.value?.action === "refund");
const payment = computed(
  () => (order.item.value.inPayments ?? []).find((p) => p.id === options.value?.paymentId) as AnyOperation | undefined,
);
const title = computed(() =>
  t(isRefund.value ? "ORDERS.PAGES.PAYMENT_ACTION.NEW_REFUND" : "ORDERS.PAGES.PAYMENT_ACTION.NEW_CAPTURE"),
);

// next free "<payment number>-NNN" transaction id, as capture-add.js / refund-add.js compute it
function nextTransactionId(p?: AnyOperation) {
  const prefix = `${p?.number ?? ""}-`;
  const children = [...(p?.captures ?? []), ...(p?.refunds ?? []), ...(p?.childrenOperations ?? [])] as AnyOperation[];
  const max = children
    .map((op) => (op.transactionId?.startsWith(prefix) ? parseInt(op.transactionId.slice(prefix.length), 10) : NaN))
    .filter((n) => !isNaN(n))
    .reduce((a, b) => Math.max(a, b), 0);
  return `${prefix}${String(max + 1).padStart(3, "0")}`;
}

interface ActionModel {
  amount?: number;
  transactionId?: string;
  reasonCode?: string;
  reasonMessage?: string;
  captureDetails?: string;
  outerId?: string;
  closeTransaction?: boolean;
}
const model = ref<ActionModel>({
  amount: payment.value?.sum,
  transactionId: nextTransactionId(payment.value),
  reasonCode: refundCodes[0],
});
const form = useBladeForm({ data: model, autoBeforeClose: false });
form.setBaseline();
const loading = ref(false);

async function submit() {
  loading.value = true;
  clearError();
  try {
    const body = { ...model.value, paymentId: options.value?.paymentId, orderId: options.value?.orderId };
    const result = await (isRefund.value ? refundPayment(body) : capturePayment(body));
    if (result?.succeeded) {
      await order.reload();
      await closeSelf();
    } else {
      setError(result?.errorMessage ?? t("ORDERS.ALERTS.PAYMENT_ACTION_FAILED"));
    }
  } catch (e) {
    setError(e);
  } finally {
    loading.value = false;
  }
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "submit",
    title: computed(() => t(isRefund.value ? "ORDERS.PAGES.PAYMENT.REFUND" : "ORDERS.PAGES.PAYMENT.CAPTURE")),
    icon: "lucide-check",
    permissions: isRefund.value ? "order:refund" : "order:capture_payment",
    disabled: computed(() => !form.formMeta.value.valid || !model.value.transactionId),
    clickHandler: submit,
  },
]);
</script>
