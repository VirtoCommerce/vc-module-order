<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.ADDRESSES')"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcDataTable
      :items="addresses"
      :state-key="'ORDER_ADDRESSES'"
      :empty-state="{ icon: 'lucide-map-pin', title: $t('ORDERS.PAGES.ADDRESSES.EMPTY') }"
      @row-click="(e: { data: AnyOperation }) => open(addresses.indexOf(e.data))"
    >
      <VcColumn
        id="name"
        :title="$t('ORDERS.PAGES.ADDRESSES.ADDRESS')"
        always-visible
      >
        <template #body="{ data: a }">{{ addressName(a) }}</template>
      </VcColumn>
      <VcColumn
        id="addressType"
        :title="$t('ORDERS.PAGES.ADDRESSES.TYPE')"
      />
      <VcColumn
        id="person"
        :title="$t('ORDERS.PAGES.ADDRESSES.PERSON')"
      >
        <template #body="{ data: a }">{{ [a.firstName, a.lastName].filter(Boolean).join(" ") }}</template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// address-list: the order's addresses; edits stay in the order until it is saved
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import type { AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderAddresses" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, exposeToChildren } = useBlade<{ orderId: string }>();
const order = useOrder(options.value?.orderId);
const addresses = computed(() => (order.item.value.addresses ?? []) as AnyOperation[]);

const addressName = (a: AnyOperation) => [a.countryCode, a.regionName, a.city, a.line1].filter(Boolean).join(", ");

const open = (index?: number) =>
  openBlade({ name: "OrderAddressDetails", options: { orderId: options.value?.orderId, index } });

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "add",
    title: t("ORDERS.PAGES.OPERATIONS.ADD"),
    icon: "lucide-plus",
    permissions: "order:update",
    disabled: order.locked,
    clickHandler: () => open(),
  },
]);

exposeToChildren({ getEntity: () => order.item.value });

// like the AngularJS list: nothing to pick from, so start a new address right away
onMounted(() => {
  if (!addresses.value.length && !order.locked.value) open();
});
</script>
