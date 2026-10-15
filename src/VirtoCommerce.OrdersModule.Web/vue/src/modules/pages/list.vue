<template>
  <VcBlade
    :title="$t('ORDERS.MENU.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="data"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :state-key="'ORDERS'"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :global-filters="globalFilters"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      row-actions-position="column"
      :empty-state="{ icon: 'lucide-shopping-cart', title: $t('ORDERS.PAGES.LIST.EMPTY') }"
      @row-click="onRowClick"
      @pagination-click="pagination.goToPage"
      @filter="onFilter"
    >
      <template #search-header-actions>
        <div class="orders-saved-filter">
          <VcSelect
            :model-value="currentId"
            :options="savedFilterOptions"
            :placeholder="$t('ORDERS.PAGES.LIST.SAVED_FILTERS.PLACEHOLDER')"
            size="small"
            @update:model-value="onSavedFilterPicked"
          />
          <VcButton
            v-if="currentFilter && !currentFilter.predefined"
            icon="lucide-pencil"
            variant="ghost"
            size="sm"
            :aria-label="$t('ORDERS.PAGES.LIST.SAVED_FILTERS.EDIT')"
            @click="openFilter(currentFilter.id)"
          />
        </div>
      </template>
      <VcColumn
        id="number"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.NUMBER')"
        body-class="orders-list__number"
        sortable
        always-visible
      />
      <VcColumn
        id="customerName"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.CUSTOMER')"
        sortable
      />
      <VcColumn
        id="storeId"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.STORE')"
        sortable
      />
      <VcColumn
        id="createdDate"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.CREATED_DATE')"
        type="date"
        sortable
      />
      <VcColumn
        id="isApproved"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.CONFIRMED')"
        :visible="false"
        sortable
      >
        <template #body="{ data: order }">{{ order.isApproved ? $t("ORDERS.YES") : $t("ORDERS.NO") }}</template>
      </VcColumn>
      <VcColumn
        id="status"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.STATUS')"
        always-visible
        sortable
      >
        <template #body="{ data: order }">
          <div class="tw-inline-flex"><VcStatus :variant="statusVariant(order.status)">{{ order.status }}</VcStatus></div>
        </template>
      </VcColumn>
      <VcColumn
        id="currency"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.CURRENCY')"
        :visible="false"
        sortable
      />
      <VcColumn
        id="total"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.TOTAL')"
        always-visible
        type="money"
        currency-field="currency"
        align="end"
        sortable
      />
      <!-- the AngularJS grid's hidden amount columns, available from the column switcher -->
      <VcColumn
        v-for="field in amountColumns"
        :id="field"
        :key="field"
        :field="field"
        :title="$t(`ORDERS.PAGES.LIST.AMOUNTS.${field}`)"
        :visible="false"
        align="end"
      >
        <template #body="{ data: order }">
          <span class="vc-table-cell-money">{{ money(order[field], order.currency, order.withPrices !== false) }}</span>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
/**
 * Extension point "orders-list-toolbar": other modules add toolbar commands to this list
 * (legacy: toolbarService.register(..., "virtoCommerce.orderModule.customerOrderListController"), e.g. AvaTax's
 * "Send to AvaTax"). How to add one: ToolbarCommand in ../utils/extensionToolbar.ts.
 *
 * ctx (OrdersListToolbarContext):
 *   selection   the selected rows (CustomerOrder: id, number, storeId, status...)
 *   keyword     the list's search keyword, undefined when empty
 *   totalCount  the number of rows in the list (all pages)
 *   openBlade   opens a blade as a child of this list
 *   reload      reloads the list
 */
import {
  useBlade,
  useDataTableSort,
  useTableSearch,
  useDataTablePagination,
  useFunctions,
  usePopup,
  notification,
  usePermissions,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcButton, VcColumn, VcDataTable, VcSelect, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import useOrdersList from "../composables/useList";
import { useSavedFilters, predefinedFilters, type SavedOrderFilter } from "../composables/useSavedFilters";
import { useSettingValues } from "../composables/useSettingValues";
import { statusVariant } from "../utils/status";
import { useMoney } from "../utils/operations";
import { useToolbarExtensions, type ToolbarCommand } from "../utils/extensionToolbar";
import { useStores } from "../composables/useStores";
import { indexedSearchEnabled } from "../api";
import { requestedKeyword } from "../search";
import type { CustomerOrder, CustomerOrderSearchCriteria } from "../../api_client/virtocommerce.orders";

export interface OrdersListToolbarContext {
  selection: CustomerOrder[];
  keyword?: string;
  totalCount: number;
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  reload: () => Promise<void>;
}
export type OrdersListToolbarCommand = ToolbarCommand<OrdersListToolbarContext>;

defineBlade({
  url: "/orders",
  name: "OrdersList",
  isWorkspace: true,
  menuItem: {
    title: "ORDERS.MENU.TITLE",
    icon: "lucide-file-text",
    priority: 10,
  },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren, closeChildren } = useBlade();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();
const money = useMoney();

const PAGE_SIZE = 20;
const amountColumns = [
  "subTotal",
  "subTotalDiscount",
  "subTotalTaxTotal",
  "shippingTotal",
  "shippingDiscountTotal",
  "paymentTotal",
  "paymentDiscountTotal",
  "discountAmount",
  "discountTotal",
  "feeTotal",
  "taxTotal",
  "sum",
];

const { sortField, sortOrder, sortExpression } = useDataTableSort({
  stateKey: "ORDERS",
  initialField: "createdDate",
  initialDirection: "DESC",
});

const { data, loading, totalCount, getItems, removeItems } = useOrdersList();
const selection = ref<CustomerOrder[]>([]);
const activeItemId = ref<string>();

const { searchValue } = useTableSearch({ stateKey: "ORDERS" });
// global search's "Show all": the list shows the keyword it was asked for, opened just now or already open
watch(
  requestedKeyword,
  (keyword) => {
    if (keyword === undefined) return;
    searchValue.value = keyword;
    requestedKeyword.value = undefined;
  },
  { immediate: true },
);
const pagination = useDataTablePagination({
  stateKey: "ORDERS",
  pageSize: PAGE_SIZE,
  totalCount,
});

const indexed = ref(false);
indexedSearchEnabled()
  .then((x) => (indexed.value = x))
  .catch(() => undefined);

// filter panel: statuses, stores, created range, prototypes
const statuses = useSettingValues("Order.Status");
const stores = useStores();
const panelFilters = ref<CustomerOrderSearchCriteria>({});

const globalFilters = computed(() => [
  {
    id: "statuses",
    label: t("ORDERS.PAGES.LIST.FILTERS.STATUS"),
    filter: { options: statuses.value.map((s) => ({ value: s.id, label: s.title })), multiple: true },
  },
  {
    id: "storeIds",
    label: t("ORDERS.PAGES.LIST.FILTERS.STORE"),
    filter: { options: stores.value.map((s) => ({ value: s.id!, label: s.name ?? s.id! })), multiple: true },
  },
  {
    id: "dateRange",
    label: t("ORDERS.PAGES.LIST.FILTERS.CREATED"),
    filter: { range: ["startDate", "endDate"] as [string, string] },
  },
  {
    id: "withPrototypes",
    label: t("ORDERS.PAGES.LIST.FILTERS.PROTOTYPES"),
    filter: { options: [{ value: "true", label: t("ORDERS.PAGES.LIST.FILTERS.WITH_PROTOTYPES") }] },
  },
]);

function asArray(v: unknown) {
  if (Array.isArray(v)) return v as string[];
  return v ? [String(v)] : undefined;
}

function onFilter(event: { filters: Record<string, unknown> }) {
  const f = event.filters;
  panelFilters.value = {
    statuses: asArray(f.statuses)?.length ? asArray(f.statuses) : undefined,
    storeIds: asArray(f.storeIds)?.length ? asArray(f.storeIds) : undefined,
    startDate: f.startDate ? new Date(f.startDate as string) : undefined,
    endDate: f.endDate ? new Date(f.endDate as string) : undefined,
    withPrototypes: f.withPrototypes ? true : undefined,
  };
  reloadFirstPage();
}

// saved filters (the AngularJS filter dropdown): predefined periods, the user's filters, "new filter"
const { filters: savedFilters, currentId, find } = useSavedFilters();
const NEW_FILTER = "__new";
const currentFilter = computed(() => find(currentId.value));
const savedFilterOptions = computed(() => [
  ...predefinedFilters().map((f) => ({ id: f.id, title: t(`ORDERS.PAGES.LIST.SAVED_FILTERS.${f.name}`) })),
  ...savedFilters.value.map((f) => ({ id: f.id, title: f.name })),
  { id: NEW_FILTER, title: t("ORDERS.PAGES.LIST.SAVED_FILTERS.NEW") },
]);

function onSavedFilterPicked(id: unknown) {
  if (id === NEW_FILTER) {
    openFilter();
    return;
  }
  currentId.value = (id as string) || undefined;
  reloadFirstPage();
}

const openFilter = (filterId?: string) => openBlade({ name: "OrderFilterDetails", options: { filterId } });

function savedCriteria(f?: SavedOrderFilter): CustomerOrderSearchCriteria {
  if (!f) return {};
  return {
    statuses: f.statuses?.length ? f.statuses : undefined,
    storeIds: f.storeIds?.length ? f.storeIds : undefined,
    customerIds: f.customerIds?.length ? f.customerIds : undefined,
    // sent too, as the AngularJS list did: with no accounts found the server filters by it (no orders), not by nothing
    customerId: f.customerId || undefined,
    organizationId: f.organizationId || undefined,
    employeeId: f.employeeId || undefined,
    startDate: f.startDate ? new Date(f.startDate) : undefined,
    endDate: f.endDate ? new Date(f.endDate) : undefined,
    withPrototypes: f.withPrototypes || undefined,
  };
}

function load() {
  const criteria = { ...savedCriteria(currentFilter.value), ...stripEmpty(panelFilters.value) };
  const filtered = Object.values(criteria).some((v) => v !== undefined);
  return getItems({
    ...criteria,
    keyword: searchValue.value || undefined,
    // the AngularJS list drops sorting for keyword (relevance) searches only when the user didn't pick one
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
    indexed: indexed.value && (!!searchValue.value || filtered),
  });
}

function stripEmpty(c: CustomerOrderSearchCriteria) {
  return Object.fromEntries(Object.entries(c).filter(([, v]) => v !== undefined)) as CustomerOrderSearchCriteria;
}

function reloadFirstPage() {
  if (pagination.skip) pagination.setPage(1);
  else load();
}

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

const reload = () => load();

async function deleteOrders(orders: CustomerOrder[]) {
  if (await showConfirmation(t("ORDERS.ALERTS.DELETE_CONFIRMATION", { count: orders.length }))) {
    await closeChildren();
    await removeItems(orders.map((x) => x.id!));
    selection.value = [];
    await reload();
  }
}

const ownToolbar = ref<IBladeToolbar[]>([
  {
    id: "refresh",
    title: t("ORDERS.PAGES.LIST.TOOLBAR.REFRESH"),
    icon: "lucide-refresh-cw",
    clickHandler: () => reload(),
  },
  {
    id: "delete",
    title: t("ORDERS.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "order:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => deleteOrders(selection.value),
  },
]);

const toolbarContext = (): OrdersListToolbarContext => ({
  selection: selection.value,
  keyword: searchValue.value || undefined,
  totalCount: totalCount.value,
  openBlade,
  reload,
});

// commands other modules add
const extensionToolbar = useToolbarExtensions("orders-list-toolbar", "Toolbar commands of the orders list", toolbarContext);
const bladeToolbar = computed<IBladeToolbar[]>(() => [...ownToolbar.value, ...extensionToolbar.value]);

async function copy(text?: string) {
  if (!text) return;
  await navigator.clipboard.writeText(text);
  notification.success(t("ORDERS.ALERTS.COPIED", { text }), { timeout: 2500 });
}

function filterBy(field: string, value?: string) {
  searchValue.value = `${field}:"${value}"`;
}

// the AngularJS row context menu
function rowActions(order: CustomerOrder) {
  return [
    { id: "copyId", icon: "lucide-copy", title: t("ORDERS.PAGES.LIST.ACTIONS.COPY_ID"), type: "info" as const, clickHandler: () => copy(order.id) },
    {
      id: "copyNumber",
      icon: "lucide-copy",
      title: t("ORDERS.PAGES.LIST.ACTIONS.COPY_NUMBER"),
      type: "info" as const,
      clickHandler: () => copy(order.number),
    },
    {
      id: "byCustomer",
      icon: "lucide-filter",
      title: t("ORDERS.PAGES.LIST.ACTIONS.FILTER_BY", { value: order.customerName }),
      type: "info" as const,
      hidden: !indexed.value || !order.customerName,
      clickHandler: () => filterBy("CustomerName", order.customerName),
    },
    {
      id: "byOrganization",
      icon: "lucide-filter",
      title: t("ORDERS.PAGES.LIST.ACTIONS.FILTER_BY", { value: order.organizationName }),
      type: "info" as const,
      hidden: !indexed.value || !order.organizationName,
      clickHandler: () => filterBy("OrganizationName", order.organizationName),
    },
    {
      id: "byStore",
      icon: "lucide-filter",
      title: t("ORDERS.PAGES.LIST.ACTIONS.FILTER_BY", { value: order.storeId }),
      type: "info" as const,
      hidden: !indexed.value || !order.storeId,
      clickHandler: () => filterBy("StoreId", order.storeId),
    },
    {
      id: "delete",
      icon: "lucide-trash-2",
      title: t("ORDERS.PAGES.LIST.TOOLBAR.DELETE"),
      type: "danger" as const,
      variant: "danger" as const,
      hidden: !hasAccess("order:delete"),
      clickHandler: () => deleteOrders([order]),
    },
  ];
}

function openDetails(item: CustomerOrder) {
  activeItemId.value = item.id;
  openBlade({
    name: "OrdersDetails",
    param: item.id,
    // the order blade reloads the list itself when it saves or deletes (as the AngularJS blades did)
    onClose: () => (activeItemId.value = undefined),
  });
}

function onRowClick(event: { data: CustomerOrder }) {
  openDetails(event.data);
}

exposeToChildren({ reload, reloadFirstPage });
</script>

<style lang="scss">
/* the design's id column is #171717; the other text columns stay #404040 */
.orders-list__number {
  color: var(--neutrals-900);
}

.orders-saved-filter {
  display: flex;
  align-items: center;
  gap: 4px;
  min-width: 200px;
}
</style>
