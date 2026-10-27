<!-- Orders to process (DashboardTables.jsx PendingOrders): the newest orders that are neither completed nor cancelled,
     with how many are waiting in all -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.TO_PROCESS')"
    icon="lucide-receipt"
    :loading="loading"
  >
    <template #actions>
      <span
        v-if="totalCount"
        class="dash-count"
        :title="$t('ORDERS.DASHBOARD.WAITING')"
      >
        {{ totalCount }}
      </span>
      <button
        type="button"
        class="dash-link-button"
        @click="openList"
      >
        {{ $t("ORDERS.DASHBOARD.VIEW_ALL") }}
      </button>
    </template>
    <template #content>
      <div
        v-if="orders.length"
        class="dash-table"
      >
        <table>
          <thead>
            <tr>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.NUMBER") }}</th>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.CREATED_DATE") }}</th>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.STATUS") }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="order in orders"
              :key="order.id"
              class="dash-table__row--link"
              @click="openOrder(order.id)"
            >
              <td>
                <span class="dash-link">{{ order.number }}</span>
              </td>
              <td class="dash-table__muted">{{ formatDate(order.createdDate) }}</td>
              <td>
                <div class="dash-inline">
                  <VcStatus :variant="statusVariant(order.status)">{{ order.status }}</VcStatus>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div
        v-else-if="!loading"
        class="dash-empty"
      >
        <VcIcon
          icon="lucide-circle-check"
          size="xl"
        />
        {{ $t("ORDERS.DASHBOARD.NOTHING_TO_PROCESS") }}
      </div>
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard, useApiClient, useAsync } from "@vc-shell/framework";
import { VcIcon, VcStatus } from "@vc-shell/framework/ui";
import { onMounted, ref } from "vue";
import { OrderModuleClient, type CustomerOrder } from "../../api_client/virtocommerce.orders";
import { statusVariant } from "../utils/status";
import { useOrderNavigation } from "./navigation";
import { formatDate } from "./statistics";

// every status of the default order workflow except Completed and Cancelled (OrdersModule ModuleConstants)
const OPEN_STATUSES = ["New", "Not payed", "Pending", "Processing", "Ready to send", "Partially sent"];

const { getApiClient } = useApiClient(OrderModuleClient);
const { openList, openOrder } = useOrderNavigation();
const orders = ref<CustomerOrder[]>([]);
const totalCount = ref(0);

const { loading, action: load } = useAsync(
  async () => {
    const client = await getApiClient();
    const result = await client.searchCustomerOrder({ statuses: OPEN_STATUSES, sort: "createdDate:desc", take: 6 });
    orders.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  },
  { notify: false },
);
onMounted(load);
</script>
