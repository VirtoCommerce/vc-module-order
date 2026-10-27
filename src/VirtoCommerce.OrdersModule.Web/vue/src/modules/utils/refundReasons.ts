import { computed } from "vue";
import { useI18n } from "vue-i18n";

// refundReasons.js
export const refundCodes = ["Duplicate", "Fraudulent", "RequestedByCustomer", "Other"];

export function useRefundReasons() {
  const { t } = useI18n({ useScope: "global" });
  return computed(() => refundCodes.map((id) => ({ id, title: t(`ORDERS.REFUND_REASONS.${id.toUpperCase()}`) })));
}
