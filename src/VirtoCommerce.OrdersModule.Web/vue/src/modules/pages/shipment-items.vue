<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.SHIPMENT_ITEMS')"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :state-key="'ORDER_SHIPMENT_ITEMS'"
      :empty-state="{ icon: 'lucide-package', title: $t('ORDERS.PAGES.OPERATIONS.EMPTY') }"
    >
      <VcColumn
        id="image"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.IMAGE')"
        width="64px"
      >
        <template #body="{ data: row }">
          <VcImage
            :src="assetUrl(row.lineItem?.imageUrl)"
            size="s"
            aspect="1x1"
            bordered
            background="contain"
          />
        </template>
      </VcColumn>
      <VcColumn
        id="name"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.NAME')"
      >
        <template #body="{ data: row }">{{ row.lineItem?.name }}</template>
      </VcColumn>
      <VcColumn
        id="status"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.STATUS')"
      />
      <VcColumn
        id="quantity"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
        type="number"
      />
      <VcColumn
        id="price"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.PRICE')"
        align="end"
      >
        <template #body="{ data: row }">
          <span class="vc-table-cell-money">{{ money(row.lineItem?.price, row.lineItem?.currency, withPrices) }}</span>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// shipment-items: order line items packed in the shipment; add from the order's items, remove selected
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcImage } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import { assetUrl, useMoney, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderShipmentItems" });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, openBlade, exposeToChildren } = useBlade<{ orderId: string }>();
const order = useOrder(options.value?.orderId);
const money = useMoney();
const withPrices = computed(() => order.item.value.withPrices !== false);

const shipment = ref<AnyOperation>();
onMounted(async () => (shipment.value = await callParent<AnyOperation>("getEntity")));
const items = computed<AnyOperation[]>(() => shipment.value?.items ?? []);
const selection = ref<AnyOperation[]>([]);
const locked = computed(() => order.locked.value || shipment.value?.status === "Send" || !!shipment.value?.isCancelled);

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "add",
    title: t("ORDERS.PAGES.SHIPMENT_ITEMS.ADD"),
    icon: "lucide-plus",
    permissions: "order:update",
    disabled: locked,
    clickHandler: () => openBlade({ name: "OrderShipmentItemsSelect", options: { orderId: options.value?.orderId } }),
  },
  {
    id: "remove",
    title: t("ORDERS.PAGES.SHIPMENT_ITEMS.REMOVE"),
    icon: "lucide-trash-2",
    permissions: "order:update",
    disabled: computed(() => locked.value || !selection.value.length),
    clickHandler: () => {
      shipment.value!.items = items.value.filter((x) => !selection.value.includes(x));
      selection.value = [];
    },
  },
]);

exposeToChildren({
  addItems(added: AnyOperation[]) {
    shipment.value!.items = [...items.value, ...added];
  },
});
</script>
