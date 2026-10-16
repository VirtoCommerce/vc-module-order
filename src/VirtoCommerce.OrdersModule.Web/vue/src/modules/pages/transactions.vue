<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.TRANSACTIONS')"
    width="50%"
  >
    <VcDataTable
      :items="transactions"
      :state-key="'ORDER_PAYMENT_TRANSACTIONS'"
      :empty-state="{ icon: 'lucide-inbox', title: $t('ORDERS.PAGES.OPERATIONS.EMPTY') }"
      @row-click="(e: { data: Record<string, unknown> }) => openBlade({ name: 'OrderJsonView', options: { title: e.data.type, data: e.data } })"
    >
      <VcColumn
        id="type"
        :title="$t('ORDERS.PAGES.TRANSACTIONS.TYPE')"
      />
      <VcColumn
        id="status"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.STATUS')"
      />
      <VcColumn
        id="responseCode"
        :title="$t('ORDERS.PAGES.TRANSACTIONS.RESPONSE_CODE')"
      />
      <VcColumn
        id="amount"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.AMOUNT')"
        align="end"
      >
        <template #body="{ data: tx }">
          <span class="vc-table-cell-money">{{ money(tx.amount, tx.currencyCode) }}</span>
        </template>
      </VcColumn>
      <VcColumn
        id="createdDate"
        :title="$t('ORDERS.PAGES.OPERATIONS.COLUMNS.CREATED')"
        type="datetime"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// payment-transactions-list: gateway transactions of the payment; a row opens its raw data
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { onMounted, ref } from "vue";
import { useMoney, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderPaymentTransactions" });

const { openBlade, callParent } = useBlade();
const money = useMoney();
const transactions = ref<AnyOperation[]>([]);
onMounted(async () => (transactions.value = (await callParent<AnyOperation>("getEntity"))?.transactions ?? []));
</script>
