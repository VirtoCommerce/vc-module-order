<!-- Recent Orders, in the design's Top Selling Products slot (DashboardTables.jsx): the platform has no
     server-side product sales aggregation, and the latest orders are what an admin acts on -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.RECENT_ORDERS')"
    icon="lucide-shopping-cart"
    :loading="loading"
  >
    <template #actions>
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
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.CUSTOMER") }}</th>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.CREATED_DATE") }}</th>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.STATUS") }}</th>
              <th class="dash-table__num">{{ $t("ORDERS.PAGES.LIST.COLUMNS.TOTAL") }}</th>
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
              <td>{{ order.customerName }}</td>
              <td class="dash-table__muted">{{ formatDate(order.createdDate) }}</td>
              <td>
                <div class="dash-inline">
                  <VcStatus :variant="statusVariant(order.status)">{{ order.status }}</VcStatus>
                </div>
              </td>
              <td class="dash-table__num dash-table__strong">{{ formatMoney(order.total ?? 0, order.currency ?? "USD", false, true) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div
        v-else-if="!loading"
        class="dash-empty"
      >
        <VcIcon
          icon="lucide-shopping-cart"
          size="xl"
        />
        {{ $t("ORDERS.DASHBOARD.NO_ORDERS") }}
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
import { formatDate, formatMoney } from "./statistics";

const { getApiClient } = useApiClient(OrderModuleClient);
const { openList, openOrder } = useOrderNavigation();
const orders = ref<CustomerOrder[]>([]);

const { loading, action: load } = useAsync(
  async () => {
    const client = await getApiClient();
    const result = await client.searchCustomerOrder({ sort: "createdDate:desc", take: 6 });
    orders.value = result.results ?? [];
  },
  { notify: false },
);
onMounted(load);
</script>
