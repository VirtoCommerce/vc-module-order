<!-- Fulfillment, in the design's Operational Metrics slot (DashboardVisuals.jsx): the share of orders from the last
     90 days that are completed and that were cancelled. The platform records no shipping times, so the design's
     second bar shows cancellations instead. -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.FULFILLMENT')"
    icon="lucide-activity"
    :loading="loading"
  >
    <template #content>
      <div class="dash-bars">
        <div class="dash-bars__row">
          <div
            v-for="bar in bars"
            :key="bar.key"
            class="dash-bars__bar"
          >
            <div
              class="dash-bars__track"
              :style="{ background: bar.track }"
            >
              <div
                class="dash-bars__fill"
                :style="{ height: `${bar.percent}%`, background: bar.fill }"
              />
              <!-- inside the fill when it is tall enough, otherwise on the track -->
              <span
                class="dash-bars__value"
                :class="{ 'dash-bars__value--outside': bar.percent < 20 }"
                :style="bar.percent < 20 ? { bottom: `calc(${bar.percent}% + 8px)` } : { top: `calc(${100 - bar.percent}% + 12px)` }"
              >
                {{ bar.percent }}%
              </span>
            </div>
            <span class="dash-bars__chip">{{ $t(`ORDERS.DASHBOARD.${bar.key}`) }}</span>
          </div>
        </div>
        <div class="dash-bars__legend">
          <div
            v-for="bar in bars"
            :key="bar.key"
            class="dash-bars__legend-row"
          >
            <span :style="{ background: bar.fill }" />
            {{ $t(`ORDERS.DASHBOARD.${bar.key}_HINT`, { count: total }) }}
          </div>
        </div>
      </div>
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard, useApiClient, useAsync } from "@vc-shell/framework";
import { computed, onMounted, ref } from "vue";
import { OrderModuleClient } from "../../api_client/virtocommerce.orders";

const { getApiClient } = useApiClient(OrderModuleClient);
const total = ref(0);
const completed = ref(0);
const cancelled = ref(0);

const { loading, action: load } = useAsync(
  async () => {
    const client = await getApiClient();
    const startDate = new Date(Date.now() - 90 * 86_400_000);
    const count = (status?: string) =>
      client.searchCustomerOrder({ startDate, status, take: 0 }).then((r) => r.totalCount ?? 0);
    [total.value, completed.value, cancelled.value] = await Promise.all([count(), count("Completed"), count("Cancelled")]);
  },
  { notify: false },
);
onMounted(load);

const percent = (part: number) => (total.value ? Math.round((part / total.value) * 100) : 0);

const bars = computed(() => [
  { key: "COMPLETED", percent: percent(completed.value), track: "#D6EBDD", fill: "#3F8F5B" },
  { key: "CANCELLED", percent: percent(cancelled.value), track: "#FBEFC8", fill: "#F0AD4E" },
]);
</script>
