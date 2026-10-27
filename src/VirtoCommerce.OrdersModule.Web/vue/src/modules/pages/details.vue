<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="orders-form tw-space-y-4">
        <div
          v-if="item.status"
          class="orders-state"
        >
          <VcStatus :variant="statusVariant(item.status)">{{ item.status }}</VcStatus>
          <VcHint
            v-if="item.isCancelled && item.cancelReason"
            >{{ item.cancelReason }}</VcHint>
        </div>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="number"
              rules="required"
              :model-value="item.number"
            >
              <VcInput
                v-model="item.number"
                :label="$t('ORDERS.PAGES.DETAILS.FIELDS.NUMBER')"
                required
                :disabled="locked"
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.status"
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.STATUS')"
              :options="statuses"
              :clearable="false"
              :disabled="locked"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <div class="orders-link-field">
              <VcLabel>{{ $t("ORDERS.PAGES.DETAILS.FIELDS.CUSTOMER") }}</VcLabel>
              <VcLink
                v-if="item.customerName"
                @click="openCustomer"
                >{{ item.customerName }}</VcLink
              >
              <span v-else>—</span>
            </div>
          </VcCol>
          <VcCol>
            <div class="orders-link-field">
              <VcLabel>{{ $t("ORDERS.PAGES.DETAILS.FIELDS.ORGANIZATION") }}</VcLabel>
              <VcLink
                v-if="item.organizationName"
                @click="openOrganization"
                >{{ item.organizationName }}</VcLink
              >
              <span v-else>—</span>
            </div>
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="storeId"
              rules="required"
              :model-value="item.storeId"
            >
              <VcSelect
                v-model="item.storeId"
                :label="$t('ORDERS.PAGES.DETAILS.FIELDS.STORE')"
                :options="stores"
                option-value="id"
                option-label="name"
                required
                :clearable="false"
                :disabled="locked"
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.employeeId"
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.EMPLOYEE')"
              :placeholder="$t('ORDERS.PAGES.DETAILS.FIELDS.EMPLOYEE_PLACEHOLDER')"
              :options="searchEmployees"
              option-value="id"
              option-label="name"
              searchable
              :disabled="locked"
              @update:model-value="async (v) => (item.employeeName = await employeeName(v))"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcField
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.CREATED')"
              :model-value="item.createdDate"
              type="date"
            />
          </VcCol>
          <VcCol>
            <VcInputCurrency
              v-if="item.withPrices !== false"
              v-model="item.discountAmount"
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.DISCOUNT')"
              :option="item.currency"
              :suffix="item.currency"
              :disabled="locked"
              @blur="recalculate()"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSwitch
              v-model="item.isApproved"
              :label="$t('ORDERS.PAGES.DETAILS.FIELDS.APPROVED')"
              :disabled="locked"
            />
          </VcCol>
          <VcCol />
        </VcRow>
        <VcTextarea
          v-model="item.comment"
          :label="$t('ORDERS.PAGES.DETAILS.FIELDS.COMMENT')"
          :disabled="locked"
        />

        <VcCard :header="$t('ORDERS.PAGES.DETAILS.ITEMS.TITLE')">
          <VcDataTable
            v-model:selection="selection"
            selection-mode="multiple"
            :items="item.items ?? []"
            edit-mode="cell"
            :state-key="'ORDER_ITEMS_V2'"
            @row-click="openLineItem"
            @cell-edit-complete="onItemEdited"
          >
            <VcColumn
              id="imageUrl"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.IMAGE')"
              width="56px"
            >
              <template #body="{ data: line }">
                <VcImage
                  :src="assetUrl(line.imageUrl)"
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
              min-width="160px"
              always-visible
            >
              <template #body="{ data: line }">
                <div class="orders-line">
                  <span class="orders-line__name">{{ line.name }}</span>
                  <span class="orders-line__sku">{{ line.sku }}</span>
                </div>
              </template>
            </VcColumn>
            <VcColumn
              id="status"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.STATUS')"
              :visible="false"
            />
            <VcColumn
              id="quantity"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
              type="number"
              width="90px"
              :editable="!locked"
            />
            <template v-if="item.withPrices !== false">
              <VcColumn
                id="price"
                :title="$t('ORDERS.PAGES.DETAILS.ITEMS.PRICE')"
                type="money"
                align="end"
                width="110px"
                :editable="!locked"
              />
              <VcColumn
                id="discountAmount"
                :title="$t('ORDERS.PAGES.DETAILS.ITEMS.DISCOUNT')"
                align="end"
                width="110px"
                :editable="!locked"
              >
                <template #body="{ data: line }">
                  <span class="vc-table-cell-money">{{ amount(line.discountAmount) }}</span>
                </template>
                <template #editor="{ data: line, editorCallback }">
                  <VcInputCurrency
                    :model-value="line.discountAmount"
                    :option="item.currency"
                    @update:model-value="editorCallback"
                  />
                </template>
              </VcColumn>
              <VcColumn
                id="taxTotal"
                :title="$t('ORDERS.PAGES.DETAILS.ITEMS.TAX')"
                align="end"
                width="100px"
              >
                <template #body="{ data: line }">
                  <span class="vc-table-cell-money">{{ amount(line.taxTotal) }}</span>
                </template>
              </VcColumn>
              <VcColumn
                id="extendedPriceWithTax"
                :title="$t('ORDERS.PAGES.DETAILS.ITEMS.TOTAL')"
                align="end"
                width="110px"
                always-visible
              >
                <template #body="{ data: line }">
                  <span class="vc-table-cell-money">{{ amount(line.extendedPriceWithTax) }}</span>
                </template>
              </VcColumn>
            </template>
          </VcDataTable>
          <dl
            v-if="item.withPrices !== false"
            class="orders-items-summary"
          >
            <template
              v-for="row in itemsSummary"
              :key="row.label"
            >
              <dt>{{ row.label }}</dt>
              <dd>{{ row.value }}</dd>
            </template>
          </dl>
        </VcCard>

        <VcRow class="tw-gap-4">
          <VcCol v-if="item.withPrices !== false">
            <VcCard :header="$t('ORDERS.PAGES.DETAILS.TOTALS.TITLE')">
              <dl class="orders-totals">
                <template
                  v-for="row in totals"
                  :key="row.label"
                >
                  <dt :class="{ 'orders-totals__grand': row.grand }">{{ row.label }}</dt>
                  <dd :class="{ 'orders-totals__grand': row.grand }">{{ amount(row.value) }}</dd>
                </template>
              </dl>
            </VcCard>
          </VcCol>
          <VcCol>
            <VcCard :header="$t('ORDERS.PAGES.DETAILS.ADDRESSES.TITLE')">
              <template #actions>
                <VcButton
                  icon="lucide-pencil"
                  variant="ghost"
                  size="sm"
                  @click="openAddresses"
                  >{{ $t("ORDERS.PAGES.DETAILS.ADDRESSES.MANAGE") }}</VcButton
                >
              </template>
              <div class="tw-p-4 tw-space-y-3 tw-text-sm">
                <div
                  v-for="(address, i) in item.addresses ?? []"
                  :key="i"
                >
                  <div class="tw-font-semibold">{{ address.addressType }}</div>
                  <div>{{ [address.firstName, address.lastName].filter(Boolean).join(" ") }}</div>
                  <div>
                    {{
                      [address.line1, address.city, address.regionName, address.postalCode, address.countryName]
                        .filter(Boolean)
                        .join(", ")
                    }}
                  </div>
                </div>
                <div v-if="!item.addresses?.length">—</div>
              </div>
            </VcCard>
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
/**
 * Extension point "orders-details-toolbar": other modules add toolbar commands to the order blade
 * (legacy: toolbarService.register(..., "virtoCommerce.orderModule.operationDetailController") for the order, e.g.
 * Return's "Create return", and on "virtoCommerce.orderModule.customerOrderItemsController", e.g. OrderManagement's
 * "Add item" / "Remove": the order's line items are a table in this blade, not a blade of their own).
 * How to add one: ToolbarCommand in ../utils/extensionToolbar.ts.
 *
 * ctx (OrdersDetailsToolbarContext):
 *   order        the order as edited in this blade (CustomerOrder, unsaved edits included); read it when you use it:
 *                the blade replaces the object on every recalculate / reload
 *   modified     true when the order has unsaved edits (read it when you use it)
 *   baseline     a copy of the order as last loaded or saved (read it when you use it)
 *   locked       true when the order is completed or cancelled (its fields are read-only)
 *   selection    the line items ticked in the order's items table (OrderLineItem)
 *   openBlade    opens a blade as a child of this order
 *   recalculate  recalculates the order's totals after an in-memory edit (e.g. items removed); Save persists it
 *   reload       reloads the order from the server (unsaved edits are dropped) and refreshes the orders list
 */
import {
  defineBladeContext,
  notification,
  useBlade,
  useBladeForm,
  useBladeWidgets,
  usePermissions,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import {
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcColumn,
  VcContainer,
  VcDataTable,
  VcField,
  VcForm,
  VcHint,
  VcImage,
  VcInput,
  VcInputCurrency,
  VcLabel,
  VcLink,
  VcRow,
  VcSelect,
  VcStatus,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import useOrdersDetails from "../composables/useDetails";
import { provideOrder } from "../composables/useOrder";
import { useCancelPrompt } from "../composables/useCancelPrompt";
import { useDynamicPropertyCount } from "../composables/useDynamicProps";
import { useSettingValues } from "../composables/useSettingValues";
import { useStores } from "../composables/useStores";
import { applyCancel, assetUrl, canCancel, clone, isLocked, useMoney } from "../utils/operations";
import { useToolbarExtensions, type ToolbarCommand } from "../utils/extensionToolbar";
import { statusVariant } from "../utils/status";
import { employeeName, getMemberByUserId, searchEmployees, type NamedEntity } from "../api";
import type { CustomerOrder, OrderLineItem } from "../../api_client/virtocommerce.orders";

defineBlade({
  url: "/order",
  name: "OrdersDetails",
});

export interface OrdersDetailsToolbarContext {
  order: CustomerOrder;
  modified: boolean;
  baseline: CustomerOrder;
  locked: boolean;
  selection: OrderLineItem[];
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  recalculate: () => Promise<void>;
  reload: () => Promise<void>;
}
export type OrdersDetailsToolbarCommand = ToolbarCommand<OrdersDetailsToolbarContext>;

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, exposeToChildren, closeSelf } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();
const askCancelReason = useCancelPrompt();
const money = useMoney();

const { item, loading, getItem, saveItem, recalculate, deleteItem } = useOrdersDetails();
const statuses = useSettingValues("Order.Status");

const form = useBladeForm({
  data: item,
  closeConfirmMessage: computed(() => t("ORDERS.ALERTS.CLOSE_CONFIRMATION")),
});
// the order as last loaded or saved (the AngularJS origEntity), for commands that merge server changes into the edits
let baseline: CustomerOrder = {};
function setBaseline() {
  form.setBaseline();
  baseline = clone(item.value);
}

const locked = computed(() => isLocked("CustomerOrder", item.value));
// ticked line items; recalculate / reload / save replace the order (and its item objects), so the ticks go with it
const selection = ref<OrderLineItem[]>([]);
watch(item, () => (selection.value = []));
const title = computed(() => item.value.number ?? t("ORDERS.PAGES.DETAILS.TITLE"));
const amount = (value?: number) => money(value, item.value.currency, item.value.withPrices !== false);

const stores = useStores();

const totals = computed(() => [
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.SUBTOTAL"), value: item.value.subTotal },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.SHIPPING"), value: item.value.shippingSubTotal },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.PAYMENT"), value: item.value.paymentSubTotal },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.TAX"), value: item.value.taxTotal },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.FEE"), value: item.value.feeTotal },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.DISCOUNT"), value: -(item.value.discountTotal ?? 0) },
  { label: t("ORDERS.PAGES.DETAILS.TOTALS.TOTAL"), value: item.value.total, grand: true },
]);

// the AngularJS line items blade summary (item totals + one line per extra currency)
const itemsSummary = computed(() => {
  const o = item.value;
  const rows = [
    { label: t("ORDERS.PAGES.DETAILS.ITEMS.SUBTOTAL"), value: amount(o.subTotal) },
    { label: t("ORDERS.PAGES.DETAILS.ITEMS.DISCOUNT_TOTAL"), value: amount(o.subTotalDiscount) },
    { label: t("ORDERS.PAGES.DETAILS.ITEMS.TAX_TOTAL"), value: amount(o.subTotalTaxTotal) },
    {
      label: t("ORDERS.PAGES.DETAILS.ITEMS.TOTAL"),
      value: amount((o.subTotalWithTax ?? 0) - (o.subTotalDiscountWithTax ?? 0)),
    },
  ];
  for (const x of o.orderTotals ?? []) {
    if (x.currencyCode !== o.currency)
      rows.push({ label: x.currencyCode ?? "", value: money(x.total, x.currencyCode) });
  }
  return rows;
});

async function onItemEdited(event: { data: OrderLineItem; field: string; newValue: unknown }) {
  (event.data as Record<string, unknown>)[event.field] = event.newValue;
  await recalculate();
}

async function save() {
  try {
    await saveItem();
    setBaseline();
    callParent("reload");
  } catch (e) {
    // concurrency conflict (another user saved the order meanwhile): reload, like the AngularJS dialog does
    if ((e as { status?: number }).status === 409) {
      showError(t("ORDERS.ALERTS.CONCURRENCY_CONFLICT"));
      await reload();
    }
  }
}

async function reload() {
  await getItem(param.value!);
  setBaseline();
}

// the order and the orders list
async function reloadAll() {
  await reload();
  callParent("reload");
}

const ownToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    disabled: computed(() => !form.canSave.value),
    permissions: "order:update",
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    permissions: "order:update",
    clickHandler: reload,
  },
  {
    id: "newDocument",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.NEW_DOCUMENT"),
    icon: "lucide-file-plus",
    disabled: computed(() => locked.value),
    permissions: "order:update",
    clickHandler: () => openBlade({ name: "OrderNewDocument", options: { orderId: item.value.id } }),
  },
  {
    id: "cancel",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.CANCEL_DOCUMENT"),
    icon: "lucide-ban",
    disabled: computed(() => !canCancel(item.value)),
    permissions: "order:update",
    async clickHandler() {
      const reason = await askCancelReason();
      if (reason === false) return;
      applyCancel(item.value, reason);
      await save();
    },
  },
  {
    id: "invoice",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.INVOICE"),
    icon: "lucide-download",
    permissions: "order:invoice:download",
    clickHandler: () => window.open(`/api/order/customerOrders/invoice/${encodeURIComponent(item.value.number!)}`, "_blank"),
  },
  {
    id: "delete",
    title: t("ORDERS.PAGES.DETAILS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "order:delete",
    async clickHandler() {
      if (await showConfirmation(t("ORDERS.ALERTS.DELETE_ORDER_CONFIRMATION", { number: item.value.number }))) {
        await deleteItem();
        form.setBaseline();
        await callParent("reload").catch(() => undefined);
        await closeSelf();
      }
    },
  },
]);

const toolbarContext = (): OrdersDetailsToolbarContext => ({
  // getters: a command that finishes later (after a picker blade) gets the order as it is then
  get order() {
    return item.value;
  },
  get modified() {
    return form.isModified.value;
  },
  get baseline() {
    return baseline;
  },
  locked: locked.value,
  selection: selection.value,
  openBlade,
  recalculate,
  reload: reloadAll,
});

// commands other modules add
const extensionToolbar = useToolbarExtensions(
  "orders-details-toolbar",
  "Toolbar commands of the order blade (and its line items table)",
  toolbarContext,
);
const bladeToolbar = computed<IBladeToolbar[]>(() => [...ownToolbar.value, ...extensionToolbar.value]);

async function openMember(load: () => Promise<NamedEntity>) {
  const member = await load().catch(() => undefined);
  if (!member?.id) {
    notification.error(t("ORDERS.ALERTS.MEMBER_NOT_FOUND"), { timeout: 5000 });
    return;
  }
  const name = member.memberType === "Organization" ? "CustomerOrganizationDetails" : "CustomerContactDetails";
  // owned by the Customer plugin; it may not be installed in this host
  await openBlade({ name, param: member.id }).catch(() =>
    notification.error(t("ORDERS.ALERTS.MEMBER_UNAVAILABLE"), { timeout: 5000 }),
  );
}
const openCustomer = () => item.value.customerId && openMember(() => getMemberByUserId(item.value.customerId!));
const openOrganization = () =>
  item.value.organizationId &&
  openMember(async () => ({ id: item.value.organizationId, memberType: "Organization" }));

function openLineItem(event: { data: OrderLineItem; originalEvent?: Event }) {
  // a click into an inline-editable cell edits it (the AngularJS inputs stopped propagation too)
  // (composedPath: the clicked cell content is already swapped for the editor when the row hears the click)
  const editable = ["quantity", "price", "discountAmount"];
  const path = (event.originalEvent?.composedPath?.() ?? []) as HTMLElement[];
  if (!locked.value && path.some((el) => editable.includes(el.dataset?.columnId ?? ""))) return;
  openBlade({
    name: "OrderLineItemDetails",
    options: { orderId: item.value.id, lineItemId: event.data.id },
  });
}

const openAddresses = () => openBlade({ name: "OrderAddresses", options: { orderId: item.value.id } });

function openOperations(kind: "payments" | "shipments") {
  openBlade({ name: "OrderOperations", options: { kind, orderId: item.value.id } });
}

const dynamicPropertyCount = useDynamicPropertyCount(computed(() => item.value.objectType));

useBladeWidgets([
  {
    id: "OrderPayments",
    icon: "lucide-credit-card",
    title: "ORDERS.WIDGETS.PAYMENTS",
    badge: computed(() => item.value.inPayments?.length ?? 0),
    onClick: () => openOperations("payments"),
  },
  {
    id: "OrderShipments",
    icon: "lucide-truck",
    title: "ORDERS.WIDGETS.SHIPMENTS",
    badge: computed(() => item.value.shipments?.length ?? 0),
    onClick: () => openOperations("shipments"),
  },
  {
    id: "OrderAddresses",
    icon: "lucide-map-pin",
    title: "ORDERS.WIDGETS.ADDRESSES",
    badge: computed(() => item.value.addresses?.length ?? 0),
    onClick: openAddresses,
  },
  {
    id: "OrderDiscounts",
    icon: "lucide-percent",
    title: "ORDERS.WIDGETS.DISCOUNTS",
    badge: computed(() => item.value.discounts?.length ?? 0),
    onClick: () => openBlade({ name: "OrderDiscounts", options: { orderId: item.value.id, path: "order" } }),
  },
  {
    id: "OrderDynamicProperties",
    icon: "lucide-list-plus",
    title: "ORDERS.WIDGETS.PROPERTIES",
    badge: dynamicPropertyCount,
    isVisible: hasAccess("platform:dynamic_properties:read"),
    onClick: () => openBlade({ name: "OrderDynamicProperties", options: { orderId: item.value.id, path: "order" } }),
  },
  {
    id: "OrderChanges",
    icon: "lucide-history",
    title: "ORDERS.WIDGETS.CHANGES",
    onClick: () => openBlade({ name: "OrderChanges", param: item.value.id }),
  },
  {
    id: "OrderNotifications",
    icon: "lucide-mail",
    title: "ORDERS.WIDGETS.NOTIFICATIONS",
    isVisible: hasAccess("notifications:access"),
    // the Notifications module's journal, limited to this order (it may not be installed in this host)
    onClick: () =>
      openBlade({
        name: "NotificationsJournal",
        options: { objectId: item.value.id, objectType: "CustomerOrder", title: item.value.number },
      }).catch(() => notification.error(t("ORDERS.ALERTS.NOTIFICATIONS_UNAVAILABLE"), { timeout: 5000 })),
  },
]);

// the order, for widgets other modules add to this blade (registerExternalWidget with targetBlades: ["OrdersDetails"])
defineBladeContext({ item });

provideOrder(param.value!, { item, locked, recalculate, reload });

onMounted(async () => {
  await getItem(param.value!);
  setBaseline();
});

exposeToChildren({
  reload: reloadAll,
  // properties / discounts / addresses blades opened from the order work on the order itself
  getEntity: () => item.value,
});
</script>

<style lang="scss">
/* the design's blade body padding (ProductDetail.jsx: 20px 22px 36px); shared by the order's child blades */
.orders-form {
  padding: 20px 22px 36px;
}

.orders-state {
  display: flex;
  align-items: center;
  gap: 12px;
}

.orders-link-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 14px;
}

.orders-line {
  display: flex;
  flex-direction: column;
  min-width: 0;
  &__name {
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    color: var(--color-neutral-900);
  }
  &__sku {
    font-size: 12px;
    color: var(--color-neutral-500);
  }
}

.orders-items-summary {
  display: grid;
  grid-template-columns: repeat(4, auto 1fr);
  gap: 4px 12px;
  margin: 0;
  padding: 12px 16px;
  font-size: 13px;
  border-top: 1px solid var(--color-neutral-200);
  dt {
    color: var(--color-neutral-500);
  }
  dd {
    margin: 0;
    font-variant-numeric: tabular-nums;
    color: var(--color-neutral-900);
  }
}

.orders-totals {
  font-size: 14px;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 8px 16px;
  margin: 0;
  padding: 16px;
  dd {
    margin: 0;
    text-align: right;
    font-variant-numeric: tabular-nums;
  }
  &__grand {
    border-top: 1px solid var(--color-neutral-200);
    padding-top: 8px;
    font-weight: 700;
    color: var(--color-neutral-950);
  }
}
</style>
