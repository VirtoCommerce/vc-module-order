<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.DISCOUNTS')"
    width="50%"
  >
    <VcDataTable
      :items="discounts"
      :state-key="'ORDER_DISCOUNTS'"
      :empty-state="{ icon: 'lucide-percent', title: $t('ORDERS.PAGES.DISCOUNTS.EMPTY') }"
    >
      <VcColumn
        id="name"
        :title="$t('ORDERS.PAGES.DISCOUNTS.NAME')"
        always-visible
      >
        <template #body="{ data: d }">{{ d.name || d.description || d.promotionId }}</template>
      </VcColumn>
      <VcColumn
        id="coupon"
        :title="$t('ORDERS.PAGES.DISCOUNTS.COUPON')"
      />
      <VcColumn
        id="discountAmount"
        :title="$t('ORDERS.PAGES.DISCOUNTS.AMOUNT')"
        align="end"
        always-visible
      >
        <template #body="{ data: d }">
          <span class="vc-table-cell-money">{{ money(d.discountAmount, d.currency, withPrices) }}</span>
        </template>
      </VcColumn>
      <VcColumn
        id="currency"
        :title="$t('ORDERS.PAGES.DISCOUNTS.CURRENCY')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// operation-discounts: promotions applied to the order / payment / shipment / line item (read only)
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useOrder } from "../composables/useOrder";
import { useMoney, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderDiscounts" });

const { options, callParent } = useBlade<{ orderId: string }>();
const order = useOrder(options.value?.orderId);
const money = useMoney();
const withPrices = computed(() => order.item.value.withPrices !== false);
const discounts = ref<AnyOperation[]>([]);
onMounted(async () => (discounts.value = (await callParent<AnyOperation>("getEntity"))?.discounts ?? []));
</script>
