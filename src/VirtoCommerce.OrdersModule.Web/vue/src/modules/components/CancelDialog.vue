<template>
  <VcPopup
    :title="$t('ORDERS.CANCEL.TITLE')"
    variant="warning"
    @close="$emit('close')"
  >
    <template #content>
      <div class="orders-cancel">
        <p>{{ $t("ORDERS.CANCEL.MESSAGE") }}</p>
        <VcTextarea
          v-model="reason"
          :label="$t('ORDERS.CANCEL.REASON')"
          :placeholder="$t('ORDERS.CANCEL.REASON_PLACEHOLDER')"
        />
      </div>
    </template>
    <template #footer="{ close }">
      <VcButton
        variant="secondary"
        @click="close()"
      >
        {{ $t("ORDERS.CANCEL.BACK") }}
      </VcButton>
      <VcButton @click="$emit('confirm', reason)">{{ $t("ORDERS.CANCEL.CONFIRM") }}</VcButton>
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
// operation cancel dialog (legacy cancelOperation-dialog.tpl.html): optional reason, confirm/back
import { VcButton, VcPopup, VcTextarea } from "@vc-shell/framework/ui";
import { ref } from "vue";

defineEmits<{ (e: "close"): void; (e: "confirm", reason: string): void }>();
const reason = ref("");
</script>

<style scoped>
.orders-cancel {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
</style>
