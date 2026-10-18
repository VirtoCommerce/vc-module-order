<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcDataTable
      :items="operations"
      :state-key="`ORDER_OPERATIONS_${kind}`"
      :active-item-id="activeItemId"
      :show-all-columns="false"
      :empty-state="{ icon: 'lucide-inbox', title: $t('ORDERS.PAGES.OPERATIONS.EMPTY') }"
      @row-click="onRowClick"
    >
      <!-- a nested blade is narrow: number, status and amount by default, the rest via the column switcher -->
      <VcColumn
        id="number"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.NUMBER')"
        always-visible
      />
      <VcColumn
        v-if="kind === 'payments'"
        id="gatewayCode"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.GATEWAY')"
      />
      <VcColumn
        v-if="kind === 'shipments'"
        id="shipmentMethodCode"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.METHOD')"
      />
      <VcColumn
        v-if="kind === 'captures' || kind === 'refunds'"
        id="transactionId"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.TRANSACTION')"
      />
      <VcColumn
        id="status"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.STATUS')"
        always-visible
      >
        <template #body="{ data: op }">
          <div class="tw-inline-flex">
            <VcStatus
              v-if="op.status"
              :variant="statusVariant(op.isCancelled ? 'Cancelled' : op.status)"
              >{{ op.status }}</VcStatus
            >
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="createdDate"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.CREATED')"
        type="date"
      />
      <!-- formatted here: the framework's money cell shows a 0 amount as "Not set" -->
      <VcColumn
        id="amount"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.AMOUNT')"
        align="end"
        always-visible
      >
        <template #body="{ data: op }">
          <span class="vc-table-cell-money">{{ amount(op) }}</span>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// payments / shipments of the order, captures / refunds of a payment (the AngularJS operations tree, per branch)
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { statusVariant } from "../utils/status";
import { KINDS, collection, useMoney, type AnyOperation, type OperationKind } from "../utils/operations";
import { useOrder } from "../composables/useOrder";

defineBlade({ name: "OrderOperations" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<{ kind: OperationKind; orderId: string; paymentId?: string }>();
const order = useOrder(options.value?.orderId);
const money = useMoney();

const kind = computed(() => options.value?.kind ?? "payments");
const operations = computed(() => collection(order.item.value, kind.value, options.value?.paymentId));
const title = computed(() => t(KINDS[kind.value].title));
const activeItemId = ref<string>();

const amount = (op: AnyOperation) =>
  money(op[KINDS[kind.value].amountField], op.currency, order.item.value.withPrices !== false);

function open(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: KINDS[kind.value].blade,
    options: { orderId: options.value?.orderId, operationId: id, isNew: !id, kind: kind.value, paymentId: options.value?.paymentId },
    onClose: () => (activeItemId.value = undefined),
  });
}

const onRowClick = (event: { data: AnyOperation }) => open(event.data.id);

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "add",
    title: t("ORDERS.PAGES.OPERATIONS.ADD"),
    icon: "lucide-plus",
    permissions: "order:update",
    isVisible: computed(() => kind.value === "payments" || kind.value === "shipments"),
    disabled: computed(() => order.locked.value),
    clickHandler: () => open(),
  },
]);
</script>
