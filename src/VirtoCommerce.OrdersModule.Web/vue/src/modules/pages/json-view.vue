<template>
  <VcBlade
    :title="String(options?.title ?? '')"
    width="40%"
  >
    <pre class="orders-json">{{ JSON.stringify(options?.data, null, 2) }}</pre>
  </VcBlade>
</template>

<script setup lang="ts">
// payment-transactions-detail: the raw transaction
import { useBlade } from "@vc-shell/framework";
import { VcBlade } from "@vc-shell/framework/ui";

defineBlade({ name: "OrderJsonView" });
const { options } = useBlade<{ title?: unknown; data?: unknown }>();
</script>

<style lang="scss">
.orders-json {
  margin: 0;
  padding: 20px 22px;
  font-size: 12px;
  white-space: pre-wrap;
  word-break: break-all;
  color: var(--color-neutral-800);
}
</style>
