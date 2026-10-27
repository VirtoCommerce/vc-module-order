<!-- Outstanding Payments (DashboardTables.jsx OutstandingPayments): incoming payments not yet captured;
     a row opens the order the payment belongs to -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.OUTSTANDING_PAYMENTS')"
    icon="lucide-credit-card"
    :loading="loading"
  >
    <template #actions>
      <span
        v-if="totalCount"
        class="dash-count"
      >
        {{ totalCount }}
      </span>
    </template>
    <template #content>
      <div
        v-if="payments.length"
        class="dash-table"
      >
        <table>
          <thead>
            <tr>
              <th>{{ $t("ORDERS.DASHBOARD.PAYMENT") }}</th>
              <th>{{ $t("ORDERS.PAGES.LIST.COLUMNS.CREATED_DATE") }}</th>
              <th class="dash-table__num">{{ $t("ORDERS.DASHBOARD.AMOUNT") }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="payment in payments"
              :key="payment.id"
              class="dash-table__row--link"
              @click="openOrder(payment.orderId)"
            >
              <td>
                <span class="dash-link">{{ payment.number }}</span>
              </td>
              <td class="dash-table__muted">{{ formatDate(payment.createdDate) }}</td>
              <td class="dash-table__num dash-table__strong">
                {{ formatMoney(payment.sum ?? 0, payment.currency ?? "USD", false, true) }}
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
        {{ $t("ORDERS.DASHBOARD.NO_OUTSTANDING_PAYMENTS") }}
      </div>
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard, useApiClient, useAsync } from "@vc-shell/framework";
import { VcIcon } from "@vc-shell/framework/ui";
import { onMounted, ref } from "vue";
import { OrderModulePaymentsClient, type PaymentIn } from "../../api_client/virtocommerce.orders";
import { useOrderNavigation } from "./navigation";
import { formatDate, formatMoney } from "./statistics";

// payment states that still expect money (PaymentStatus: New, Pending, Authorized; Paid and later are settled)
const OUTSTANDING = ["New", "Pending", "Authorized"];

const { getApiClient } = useApiClient(OrderModulePaymentsClient);
const { openOrder } = useOrderNavigation();
const payments = ref<PaymentIn[]>([]);
const totalCount = ref(0);

const { loading, action: load } = useAsync(
  async () => {
    const client = await getApiClient();
    const result = await client.searchOrderPayments({ statuses: OUTSTANDING, sort: "createdDate:desc", take: 5 });
    payments.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  },
  { notify: false },
);
onMounted(load);
</script>
