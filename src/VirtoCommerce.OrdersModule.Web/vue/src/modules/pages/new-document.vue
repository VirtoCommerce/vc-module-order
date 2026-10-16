<template>
  <VcBlade
    :title="$t('ORDERS.PAGES.NEW_DOCUMENT.TITLE')"
    width="30%"
  >
    <VcDataTable
      :items="choices"
      data-key="blade"
      :state-key="'ORDER_NEW_DOCUMENT'"
      :column-switcher="false"
      @row-click="(e: { data: Choice }) => replaceWith({ name: e.data.blade, options: { orderId: options?.orderId, isNew: true } })"
    >
      <VcColumn
        id="title"
        :title="$t('ORDERS.PAGES.NEW_DOCUMENT.TYPE')"
      >
        <template #body="{ data: choice }">
          <div class="orders-line">
            <span class="orders-line__name">{{ choice.title }}</span>
            <span class="orders-line__sku">{{ choice.description }}</span>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// the AngularJS newOperation wizard: pick the type of the child document to add to the order
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";

defineBlade({ name: "OrderNewDocument" });

interface Choice {
  blade: string;
  title: string;
  description: string;
}

const { t } = useI18n({ useScope: "global" });
const { options, replaceWith } = useBlade<{ orderId: string }>();

const choices = computed<Choice[]>(() => [
  {
    blade: "OrderShipmentDetails",
    title: t("ORDERS.PAGES.NEW_DOCUMENT.SHIPMENT"),
    description: t("ORDERS.PAGES.NEW_DOCUMENT.SHIPMENT_DESCR"),
  },
  {
    blade: "OrderPaymentDetails",
    title: t("ORDERS.PAGES.NEW_DOCUMENT.PAYMENT"),
    description: t("ORDERS.PAGES.NEW_DOCUMENT.PAYMENT_DESCR"),
  },
]);
</script>
