<!-- Total Sales (Dashboard.jsx TotalSalesWidget): revenue, orders and average order value
     for the chosen period, each compared with the period before it -->
<template>
  <DashboardWidgetCard
    :header="$t('ORDERS.DASHBOARD.TOTAL_SALES')"
    icon="lucide-chart-no-axes-column"
    :loading="loading"
  >
    <template #actions>
      <select
        v-model="period"
        class="dash-select"
        :aria-label="$t('ORDERS.DASHBOARD.PERIOD')"
      >
        <option
          v-for="(days, key) in PERIODS"
          :key="key"
          :value="key"
        >
          {{ $t(`ORDERS.DASHBOARD.PERIODS.${key.toUpperCase()}`) }}
        </option>
      </select>
    </template>
    <template #content>
      <div class="dash-metrics">
        <div
          v-for="metric in metrics"
          :key="metric.key"
          class="dash-metric"
        >
          <span
            class="dash-metric__icon"
            :class="`dash-metric__icon--${metric.tint}`"
          >
            <VcIcon :icon="metric.icon" />
          </span>
          <div class="dash-metric__body">
            <div class="dash-metric__label">{{ $t(`ORDERS.DASHBOARD.${metric.key}`) }}</div>
            <div class="dash-metric__line">
              <span class="dash-metric__value">{{ metric.value }}</span>
              <span
                v-if="metric.delta"
                class="dash-delta"
                :class="{ 'dash-delta--down': metric.down }"
                :title="metric.percent"
              >
                {{ metric.delta }}
                <VcIcon :icon="metric.down ? 'lucide-trending-down' : 'lucide-trending-up'" />
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard } from "@vc-shell/framework";
import { VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import type { DashboardStatisticsResult } from "../../api_client/virtocommerce.orders";
import { amountIn, daysBefore, endOfToday, formatMoney, getStatistics, ltr, mainCurrency } from "./statistics";

const PERIODS = { year: 365, quarter: 90, month: 30 } as const;
type Period = keyof typeof PERIODS;

const period = ref<Period>("year");
const loading = ref(false);
const current = ref<DashboardStatisticsResult>({});
const previous = ref<DashboardStatisticsResult>({});

watch(
  period,
  async (value) => {
    loading.value = true;
    try {
      const end = endOfToday();
      const start = daysBefore(end, PERIODS[value]);
      [current.value, previous.value] = await Promise.all([
        getStatistics(start, end),
        getStatistics(daysBefore(start, PERIODS[value]), start),
      ]);
    } finally {
      loading.value = false;
    }
  },
  { immediate: true },
);

const formatCount = (value: number, signed = false) =>
  ltr(new Intl.NumberFormat(undefined, { signDisplay: signed ? "exceptZero" : "auto" }).format(value));

const metrics = computed(() => {
  const currency = mainCurrency([current.value, previous.value]);
  const rows = [
    {
      key: "REVENUE",
      icon: "lucide-shopping-cart",
      tint: "blue",
      now: amountIn(current.value.revenue, currency),
      before: amountIn(previous.value.revenue, currency),
      format: (v: number, signed?: boolean) => formatMoney(v, currency, signed),
    },
    {
      key: "ORDERS",
      icon: "lucide-receipt",
      tint: "amber",
      now: current.value.orderCount ?? 0,
      before: previous.value.orderCount ?? 0,
      format: formatCount,
    },
    {
      key: "AVG_ORDER_VALUE",
      icon: "lucide-package",
      tint: "green",
      now: amountIn(current.value.avgOrderValue, currency),
      before: amountIn(previous.value.avgOrderValue, currency),
      format: (v: number, signed?: boolean) => formatMoney(v, currency, signed),
    },
  ];
  return rows.map((row) => {
    const change = row.now - row.before;
    return {
      key: row.key,
      icon: row.icon,
      tint: row.tint,
      value: row.format(row.now),
      delta: change ? row.format(change, true) : "",
      down: change < 0,
      percent: row.before ? `${formatCount(Math.round((change / row.before) * 100), true)}%` : undefined,
    };
  });
});
</script>
