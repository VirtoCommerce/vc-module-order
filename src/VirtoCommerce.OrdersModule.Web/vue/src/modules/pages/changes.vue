<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.CHANGES')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="60%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="changes"
      :total-count="totalCount"
      :pagination="isOrder ? pagination : undefined"
      :searchable="isOrder"
      :global-filters="isOrder ? globalFilters : undefined"
      :state-key="'ORDER_CHANGES_V3'"
      :empty-state="{ icon: 'lucide-history', title: $t('ORDERS.PAGES.CHANGES.EMPTY') }"
      @pagination-click="pagination.goToPage"
      @filter="onFilter"
    >
      <VcColumn
        id="operationType"
        :title="$t('ORDERS.PAGES.CHANGES.COLUMNS.TYPE')"
        width="100px"
      />
      <VcColumn
        id="createdBy"
        :title="$t('ORDERS.PAGES.CHANGES.COLUMNS.USER')"
        width="110px"
      />
      <VcColumn
        id="detail"
        :title="$t('ORDERS.PAGES.CHANGES.COLUMNS.DETAIL')"
        always-visible
      />
      <VcColumn
        id="createdDate"
        :title="$t('ORDERS.PAGES.CHANGES.COLUMNS.DATE')"
        type="datetime"
        :sortable="isOrder"
        width="170px"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// customerOrder-change-log (order: searchChanges with keyword / type / date filters and paging) and the platform
// operations widget of payments, shipments, captures and refunds (changes of that one entity)
import {
  ChangeLogClient,
  useApiClient,
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { OrderModuleClient, type OperationLog } from "../../api_client/virtocommerce.orders";

defineBlade({
  url: "/order-changes",
  name: "OrderChanges",
});

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { param, options } = useBlade<{ objectType?: string }>();
const { getApiClient } = useApiClient(OrderModuleClient);
const { getApiClient: getChangeLog } = useApiClient(ChangeLogClient);
const { debounce } = useFunctions();

// an operation (payment, shipment...) passes its type; the order's own log is searchable
const isOrder = computed(() => !options.value?.objectType);
const changes = ref<OperationLog[]>([]);
const totalCount = ref(0);
const keyword = ref("");
const filters = ref<{ operationTypes?: string[]; startDate?: Date; endDate?: Date }>({});
const { sortField, sortOrder, sortExpression } = useDataTableSort({
  initialField: "createdDate",
  initialDirection: "DESC",
});
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  if (isOrder.value) {
    const result = await (
      await getApiClient()
    ).searchOrderChanges({
      orderId: param.value,
      keyword: keyword.value || undefined,
      sort: sortExpression.value,
      skip: pagination.skip,
      take: PAGE_SIZE,
      ...filters.value,
    });
    changes.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  } else {
    changes.value = ((await (
      await getChangeLog()
    ).searchChanges({ objectIds: [param.value!], objectType: options.value?.objectType })) ?? []) as OperationLog[];
    totalCount.value = changes.value.length;
  }
});

const globalFilters = computed(() => [
  {
    id: "operationType",
    label: t("ORDERS.PAGES.CHANGES.COLUMNS.TYPE"),
    filter: { options: ["Added", "Modified", "Deleted"].map((x) => ({ value: x, label: x })) },
  },
  {
    id: "dateRange",
    label: t("ORDERS.PAGES.CHANGES.COLUMNS.DATE"),
    filter: { range: ["startDate", "endDate"] as [string, string] },
  },
]);

function onFilter(event: { filters: Record<string, unknown> }) {
  const f = event.filters;
  filters.value = {
    operationTypes: f.operationType ? [f.operationType as string] : undefined,
    startDate: f.startDate ? new Date(f.startDate as string) : undefined,
    endDate: f.endDate ? new Date(f.endDate as string) : undefined,
  };
  // off the first page, the page change loads
  if (pagination.skip) pagination.setPage(1);
  else load();
}

onMounted(load);
watch(keyword, () => pagination.setPage(1));
watch([sortExpression, keyword, () => pagination.skip], debounce(load, 300));

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("ORDERS.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);
</script>
