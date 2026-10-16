<template>
  <VcBlade
    :title="line?.name ?? ''"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcContainer v-if="line">
      <VcForm class="orders-form tw-space-y-4">
        <div class="orders-link-field">
          <VcLabel>{{ $t("ORDERS.PAGES.LINE_ITEM.PRODUCT") }}</VcLabel>
          <VcLink @click="openProduct">{{ line.name }}</VcLink>
          <VcHint
            v-if="line.comment"
            >{{ line.comment }}</VcHint>
        </div>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcField
              :label="$t('ORDERS.PAGES.DETAILS.ITEMS.SKU')"
              :model-value="line.sku"
              copyable
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="line.status"
              :label="$t('ORDERS.PAGES.DETAILS.ITEMS.STATUS')"
              :options="statuses"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="line.quantity"
              type="number"
              min="1"
              :label="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
              :disabled="locked"
              @blur="recalculate"
            />
          </VcCol>
          <VcCol />
        </VcRow>
        <template v-if="withPrices">
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcInputCurrency
                v-model="line.price"
                :label="$t('ORDERS.PAGES.DETAILS.ITEMS.PRICE')"
                :option="line.currency"
                :suffix="line.currency"
                :disabled="locked"
                @blur="recalculate"
              />
            </VcCol>
            <VcCol>
              <VcField
                :label="$t('ORDERS.PAGES.LINE_ITEM.PRICE_WITH_TAX')"
                :model-value="money(line.priceWithTax, line.currency)"
              />
            </VcCol>
          </VcRow>
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcInputCurrency
                v-model="line.discountAmount"
                :label="$t('ORDERS.PAGES.DETAILS.ITEMS.DISCOUNT')"
                :option="line.currency"
                :suffix="line.currency"
                :disabled="locked"
                @blur="recalculate"
              />
            </VcCol>
            <VcCol>
              <VcField
                :label="$t('ORDERS.PAGES.LINE_ITEM.DISCOUNT_WITH_TAX')"
                :model-value="money(line.discountAmountWithTax, line.currency)"
              />
            </VcCol>
          </VcRow>
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcField
                :label="$t('ORDERS.PAGES.DETAILS.ITEMS.TAX')"
                :model-value="money(line.taxTotal, line.currency)"
              />
            </VcCol>
            <VcCol>
              <VcField
                :label="$t('ORDERS.PAGES.DETAILS.ITEMS.TOTAL')"
                :model-value="money(line.extendedPriceWithTax, line.currency)"
              />
            </VcCol>
          </VcRow>
        </template>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// customerOrder-item-detail: edits the line item inside the open order; quantity / price / discount recalculate totals
/**
 * Extension point "orders-line-item-toolbar": other modules add toolbar commands to the line item blade
 * (legacy: toolbarService.register(..., "virtoCommerce.orderModule.customerOrderItemDetailController"), e.g.
 * ProductSnapshot's "Product snapshot"). How to add one: ToolbarCommand in ../utils/extensionToolbar.ts.
 *
 * ctx (OrdersLineItemToolbarContext):
 *   order      the order the line item belongs to, as edited in the order blade (CustomerOrder, unsaved edits included)
 *   item       the line item (OrderLineItem); undefined once it is removed from the order
 *   locked     true when the order is completed or cancelled (read-only)
 *   openBlade  opens a blade as a child of this line item
 *   reload     reloads the order from the server (unsaved edits are dropped)
 * The blade context (defineBladeContext) is { item }: the line item, for widgets other modules add here.
 */
import {
  defineBladeContext,
  notification,
  useBlade,
  useBladeWidgets,
  usePermissions,
} from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcHint,
  VcInput,
  VcInputCurrency,
  VcLabel,
  VcLink,
  VcRow,
  VcSelect,
} from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useOrder } from "../composables/useOrder";
import { useSettingValues } from "../composables/useSettingValues";
import { useDynamicPropertyCount } from "../composables/useDynamicProps";
import { useMoney } from "../utils/operations";
import { useToolbarExtensions, type ToolbarCommand } from "../utils/extensionToolbar";
import type { CustomerOrder, OrderLineItem } from "../../api_client/virtocommerce.orders";

defineBlade({ name: "OrderLineItemDetails" });

export interface OrdersLineItemToolbarContext {
  order: CustomerOrder;
  item?: OrderLineItem;
  locked: boolean;
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  reload: () => Promise<void>;
}
export type OrdersLineItemToolbarCommand = ToolbarCommand<OrdersLineItemToolbarContext>;

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, exposeToChildren } = useBlade<{ orderId: string; lineItemId: string }>();
const { hasAccess } = usePermissions();
const order = useOrder(options.value?.orderId);
const money = useMoney();
const statuses = useSettingValues("OrderLineItem.Statuses");

// re-resolved after every recalculation, which replaces the order object
const line = computed(() => (order.item.value.items ?? []).find((x) => x.id === options.value?.lineItemId));
const locked = order.locked;
const withPrices = computed(() => order.item.value.withPrices !== false);
const recalculate = () => order.recalculate();

async function openProduct() {
  // the Catalog plugin owns the product blade; it may not be installed in this host
  await openBlade({ name: "CatalogProductDetails", param: line.value?.productId }).catch(() =>
    notification.error(t("ORDERS.ALERTS.PRODUCT_UNAVAILABLE"), { timeout: 5000 }),
  );
}

const dynamicPropertyCount = useDynamicPropertyCount(computed(() => line.value?.objectType));
const openChild = (name: string) => openBlade({ name, options: { orderId: options.value?.orderId } });

useBladeWidgets([
  {
    id: "LineItemDiscounts",
    icon: "lucide-percent",
    title: "ORDERS.WIDGETS.DISCOUNTS",
    badge: computed(() => line.value?.discounts?.length ?? 0),
    onClick: () => openChild("OrderDiscounts"),
  },
  {
    id: "LineItemProperties",
    icon: "lucide-list-plus",
    title: "ORDERS.WIDGETS.PROPERTIES",
    badge: dynamicPropertyCount,
    isVisible: hasAccess("platform:dynamic_properties:read"),
    onClick: () => openChild("OrderDynamicProperties"),
  },
  {
    id: "LineItemConfiguration",
    icon: "lucide-sliders-horizontal",
    title: "ORDERS.WIDGETS.CONFIGURATION",
    badge: computed(() => line.value?.configurationItems?.length ?? 0),
    isVisible: computed(() => !!line.value?.configurationItems?.length && hasAccess("catalog:configurations:read")),
    onClick: () => openChild("OrderItemConfiguration"),
  },
]);

const toolbarContext = (): OrdersLineItemToolbarContext => ({
  order: order.item.value,
  item: line.value,
  locked: locked.value,
  openBlade,
  reload: order.reload,
});

// the blade has no commands of its own: only those other modules add
const bladeToolbar = useToolbarExtensions(
  "orders-line-item-toolbar",
  "Toolbar commands of the order line item blade",
  toolbarContext,
);

// the line item, for widgets other modules add to this blade (registerExternalWidget with targetBlades: ["OrderLineItemDetails"])
defineBladeContext({ item: line });

exposeToChildren({ getEntity: () => line.value });
</script>
