<template>
  <VcWidget
    icon="lucide-file-text"
    :title="$t('ORDERS.WIDGETS.MEMBER_ORDERS')"
    :value="count ?? '…'"
    :disabled="!criteria"
    @click="open"
  />
</template>

<script setup lang="ts">
// customerOrder-list-widget on the Customer module's contact / organization blades:
// orders of the contact's (first) account, or of the organization
import { computed, ref, watch } from "vue";
import { injectBladeContext, useApiClient, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { OrderModuleClient } from "../../api_client/virtocommerce.orders";
import type { NamedEntity } from "../api";

const ctx = injectBladeContext();
const { openBlade } = useBlade();
const { getApiClient } = useApiClient(OrderModuleClient);
const count = ref<number>();

const criteria = computed(() => {
  const member = ctx.value.item as NamedEntity | undefined;
  if (!member?.id) return undefined;
  if (member.memberType === "Organization") return { organizationId: member.id };
  const accountId = member.securityAccounts?.[0]?.id;
  return accountId ? { customerId: accountId } : undefined;
});

watch(
  criteria,
  async (c) => {
    count.value = undefined;
    if (!c) return;
    const result = await (await getApiClient()).searchCustomerOrder({ ...c, responseGroup: "Default", take: 0 });
    count.value = result.totalCount ?? 0;
  },
  { immediate: true },
);

const open = () => criteria.value && openBlade({ name: "OrdersMemberOrders", options: { ...criteria.value } });
</script>
