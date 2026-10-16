<template>
  <VcBlade
    :title="$t('ORDERS.PAGES.SHIPMENT_ITEMS.SELECT_TITLE')"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      edit-mode="cell"
      :state-key="'ORDER_SHIPMENT_ITEMS_SELECT'"
      @cell-edit-complete="(e: { data: AnyOperation; field: string; newValue: unknown }) => (e.data[e.field] = e.newValue)"
    >
      <VcColumn
        id="imageUrl"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.IMAGE')"
        width="64px"
      >
        <template #body="{ data: row }">
          <VcImage
                  :src="assetUrl(row.imageUrl)"
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
      />
      <VcColumn
        id="quantityOnShipment"
        :title="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
        type="number"
        editable
      />
      <VcColumn
        id="quantity"
        :title="$t('ORDERS.PAGES.SHIPMENT_ITEMS.AVAILABLE')"
        type="number"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// customerOrder-items-select: pick order line items (and the quantity to ship) for the shipment
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcImage } from "@vc-shell/framework/ui";

import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import { assetUrl, clone, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderShipmentItemsSelect" });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ orderId: string }>();
const order = useOrder(options.value?.orderId);

const rows = ref<AnyOperation[]>(
  clone((order.item.value.items ?? []) as AnyOperation[]).map((x: AnyOperation) => ({ ...x, quantityOnShipment: x.quantity })),
);
const selection = ref<AnyOperation[]>([]);

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "add",
    title: t("ORDERS.PAGES.SHIPMENT_ITEMS.ADD"),
    icon: "lucide-plus",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      const added = selection.value.map(({ quantityOnShipment, ...lineItem }) => ({
        lineItemId: lineItem.id,
        lineItem,
        quantity: quantityOnShipment,
      }));
      await callParent("addItems", added);
      await closeSelf();
    },
  },
]);
</script>
