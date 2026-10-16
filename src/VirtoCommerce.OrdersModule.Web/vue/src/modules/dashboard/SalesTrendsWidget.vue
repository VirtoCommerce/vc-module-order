<!-- Sales Trends (Dashboard.jsx SalesTrendsWidget): revenue for the last 12 months or 8 quarters,
     headline = the latest period against the one before -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.SALES_TRENDS')"
    icon="lucide-trending-up"
    :loading="loading"
  >
    <template #actions>
      <span
        v-if="headline"
        class="dash-delta dash-delta--headline"
        :class="{ 'dash-delta--down': headline.down }"
      >
        {{ headline.text }}
        <VcIcon :icon="headline.down ? 'lucide-trending-down' : 'lucide-trending-up'" />
      </span>
      <select
        v-model="granularity"
        class="dash-select"
        :aria-label="$t('ORDERS.DASHBOARD.PERIOD')"
      >
        <option value="month">{{ $t("ORDERS.DASHBOARD.PERIODS.MONTH") }}</option>
        <option value="quarter">{{ $t("ORDERS.DASHBOARD.PERIODS.QUARTER") }}</option>
      </select>
    </template>
    <template #content>
      <TrendChart
        :values="values"
        :labels="buckets.map((b) => b.label)"
        :format="(v: number, signed?: boolean) => formatMoney(v, currency, signed)"
        :aria-label="$t('ORDERS.DASHBOARD.SALES_TRENDS')"
      />
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard } from "@vc-shell/framework";
import { VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import type { DashboardStatisticsResult } from "../../api_client/virtocommerce.orders";
import TrendChart from "./TrendChart.vue";
import { amountIn, formatMoney, getStatistics, mainCurrency, monthBuckets, quarterBuckets } from "./statistics";

const granularity = ref<"month" | "quarter">("month");
const loading = ref(false);
const results = ref<DashboardStatisticsResult[]>([]);

const buckets = computed(() => (granularity.value === "month" ? monthBuckets(12) : quarterBuckets(8)));

watch(
  buckets,
  async (list) => {
    // the previous period's results don't fit the new buckets (12 months vs one quarters response)
    results.value = [];
    loading.value = true;
    try {
      // ponytail: one request per month (cached here and on the server); a series endpoint would make it one
      const next =
        granularity.value === "month"
          ? await Promise.all(list.map((b) => getStatistics(b.start, b.end)))
          : [await getStatistics(list[0].start, list[list.length - 1].end)];
      // a quicker later switch already owns the chart
      if (list !== buckets.value) return;
      results.value = next;
    } finally {
      if (list === buckets.value) loading.value = false;
    }
  },
  { immediate: true },
);

const currency = computed(() => mainCurrency(results.value));

const values = computed(() => {
  if (granularity.value === "month") {
    return results.value.map((r) => amountIn(r.revenue, currency.value));
  }
  // quarters come from one response's per-quarter details
  const details = results.value[0]?.revenuePeriodDetails ?? [];
  return buckets.value.map((b) => {
    const year = b.start.getUTCFullYear();
    const quarter = Math.floor(b.start.getUTCMonth() / 3) + 1;
    return details.find((d) => d.year === year && d.quarter === quarter && d.currency === currency.value)?.amount ?? 0;
  });
});

const headline = computed(() => {
  const [before, last] = values.value.slice(-2);
  const change = (last ?? 0) - (before ?? 0);
  return change ? { text: formatMoney(change, currency.value, true), down: change < 0 } : null;
});
</script>

<style lang="scss">
.dash-delta--headline {
  font-size: 16px;
  font-weight: 700;
}
</style>
