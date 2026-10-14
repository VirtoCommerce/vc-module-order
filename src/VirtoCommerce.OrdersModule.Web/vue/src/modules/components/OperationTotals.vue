<template>
  <VcCard :header="$t('ORDERS.PAGES.DETAILS.TOTALS.TITLE')">
    <dl class="orders-totals">
      <dt>{{ $t("ORDERS.PAGES.OPERATION.PRICE") }}</dt>
      <dd>{{ money(operation.price, operation.currency) }}</dd>
      <dt>{{ $t("ORDERS.PAGES.DETAILS.TOTALS.DISCOUNT") }}</dt>
      <dd>{{ money(-(operation.discountAmount ?? 0), operation.currency) }}</dd>
      <dt>{{ $t("ORDERS.PAGES.DETAILS.TOTALS.TAX") }}</dt>
      <dd>{{ money(operation.taxTotal, operation.currency) }}</dd>
      <dt class="orders-totals__grand">{{ $t("ORDERS.PAGES.DETAILS.TOTALS.TOTAL") }}</dt>
      <dd class="orders-totals__grand">{{ money(operation.totalWithTax, operation.currency) }}</dd>
    </dl>
  </VcCard>
</template>

<script setup lang="ts">
// payment / shipment totals widget of the AngularJS blades (price, discount, tax, total with tax)
import { VcCard } from "@vc-shell/framework/ui";
import { useMoney, type AnyOperation } from "../utils/operations";

defineProps<{ operation: AnyOperation }>();
const money = useMoney();
</script>
