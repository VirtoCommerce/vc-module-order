<!-- A date column's cell: the date, and the time under it, smaller and muted, so the column stays narrow (VCST-6559).
     A stopgap until vc-shell's VcColumn has a two-line "datetime" type (VCST-6956). -->
<template>
  <span
    v-if="value"
    class="orders-datetime-cell"
    :title="dateTime(value)"
  >
    <span>{{ date(value) }}</span>
    <span class="orders-datetime-cell__time">{{ time(value) }}</span>
  </span>
</template>

<script lang="ts" setup>
import { useDateTime } from "../utils/dateTime";

defineProps<{ value?: Date | string | null }>();

const { date, time, dateTime } = useDateTime();
</script>

<style lang="scss">
.orders-datetime-cell {
  display: inline-flex;
  flex-direction: column;
  line-height: 1.25;
  white-space: nowrap;

  &__time {
    color: var(--fg-2, var(--neutrals-500));
    font-size: 12px;
  }
}
</style>
