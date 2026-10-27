<template>
  <VcBlade
    :title="$t('ORDERS.PAGES.LIST.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="data"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :active-item-id="activeItemId"
      :state-key="'MEMBER_ORDERS'"
      :empty-state="{ icon: 'lucide-shopping-cart', title: $t('ORDERS.PAGES.LIST.EMPTY') }"
      @row-click="open"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="number"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.NUMBER')"
        sortable
        always-visible
      />
      <VcColumn
        id="status"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.STATUS')"
        sortable
        always-visible
      >
        <template #body="{ data: order }">
          <div class="tw-inline-flex">
            <VcStatus :variant="statusVariant(order.status)">{{ order.status }}</VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="createdDate"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.CREATED_DATE')"
        type="date"
        sortable
      />
      <VcColumn
        id="total"
        :title="$t('ORDERS.PAGES.LIST.COLUMNS.TOTAL')"
        align="end"
        sortable
        always-visible
      >
        <template #body="{ data: order }">
          <span class="vc-table-cell-money">{{ money(order.total, order.currency, order.withPrices !== false) }}</span>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// the orders list the AngularJS customer/organization widget opens (customerOrderListController with searchCriteria)
import {
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import useOrdersList from "../composables/useList";
import { statusVariant } from "../utils/status";
import { useMoney } from "../utils/operations";
import type { CustomerOrder, CustomerOrderSearchCriteria } from "../../api_client/virtocommerce.orders";

defineBlade({ name: "OrdersMemberOrders" });

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
// options are search criteria: { customerId } / { organizationId } from the member widget; other modules may pass
// any other criteria (e.g. { promotionId }), like the AngularJS list's searchCriteria
const { options, openBlade, exposeToChildren } = useBlade<CustomerOrderSearchCriteria>();
const { debounce } = useFunctions();
const money = useMoney();
const { data, loading, totalCount, getItems } = useOrdersList();
const keyword = ref("");
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "createdDate", initialDirection: "DESC" });
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const load = () =>
  getItems({
    ...options.value,
    keyword: keyword.value || undefined,
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });

load();
watch(keyword, () => pagination.setPage(1));
watch([sortExpression, keyword, () => pagination.skip], debounce(load, 300));

function open(event: { data: CustomerOrder }) {
  activeItemId.value = event.data.id;
  openBlade({ name: "OrdersDetails", param: event.data.id, onClose: () => (activeItemId.value = undefined) });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("ORDERS.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);

exposeToChildren({ reload: load });
</script>
