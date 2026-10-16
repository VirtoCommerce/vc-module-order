import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";

import { useApiClient, useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { OrderModuleClient } from "../../api_client/virtocommerce.orders";
import { useOrder } from "./useOrder";
import { useCancelPrompt } from "./useCancelPrompt";
import { KINDS, applyCancel, canCancel, clone, collection, isLocked, type AnyOperation, type OperationKind } from "../utils/operations";

export interface OperationBladeOptions {
  orderId: string;
  operationId?: string;
  paymentId?: string;
  kind?: OperationKind;
  isNew?: boolean;
}

/**
 * Edit a copy of a payment / shipment / capture / refund of the open order (AngularJS operation-detail.js):
 * OK writes it back into the order and closes (recalculating totals when the price changed), Delete removes it
 * from the order, Cancel marks it cancelled. Nothing is persisted until the order blade is saved.
 */
export function useOperationEditor(fixedKind?: OperationKind) {
  const { t } = useI18n({ useScope: "global" });
  const { options, closeSelf, exposeToChildren } = useBlade<OperationBladeOptions>();
  const { getApiClient } = useApiClient(OrderModuleClient);
  const { showConfirmation } = usePopup();
  const askCancelReason = useCancelPrompt();

  const kind = (fixedKind ?? options.value?.kind ?? "payments") as OperationKind;
  const order = useOrder(options.value?.orderId);
  const isNew = ref(!!options.value?.isNew);
  const entity = ref<AnyOperation>({});
  const loading = ref(false);
  const operationType = KINDS[kind].operationType;

  const list = () => collection(order.item.value, kind, options.value?.paymentId);
  const original = () => list().find((x) => x.id === entity.value.id);
  const locked = computed(() => order.locked.value || isLocked(operationType, entity.value));
  const withPrices = computed(() => order.item.value.withPrices !== false);

  const form = useBladeForm({ data: entity, autoBeforeClose: true });

  onMounted(async () => {
    loading.value = true;
    try {
      if (isNew.value) {
        const client = await getApiClient();
        const id = order.item.value.id!;
        const prototype = kind === "payments" ? await client.getNewPayment(id) : await client.getNewShipment(id);
        entity.value = { ...prototype, createdDate: undefined } as AnyOperation;
      } else {
        entity.value = clone(list().find((x) => x.id === options.value?.operationId) ?? {});
      }
    } finally {
      loading.value = false;
      form.setBaseline();
    }
  });

  // the order was recalculated / reloaded (e.g. after a capture): refresh an untouched copy
  watch(
    () => order.item.value,
    () => {
      const base = original();
      if (base && !isNew.value && !form.isModified.value) {
        entity.value = clone(base);
        form.setBaseline();
      }
    },
  );

  async function apply() {
    const target = list();
    const index = target.findIndex((x) => x.id === entity.value.id);
    const before = index >= 0 ? target[index] : undefined;
    const priceChanged = !before || before.price !== entity.value.price || before.priceWithTax !== entity.value.priceWithTax;
    if (index >= 0) target.splice(index, 1, clone(entity.value));
    else target.push(clone(entity.value));
    form.setBaseline();
    await closeSelf();
    if (priceChanged && (kind === "payments" || kind === "shipments")) await order.recalculate();
  }

  const toolbar: IBladeToolbar[] = [
    {
      id: "apply",
      title: t("ORDERS.PAGES.OPERATION.TOOLBAR.APPLY"),
      icon: "lucide-check",
      permissions: "order:update",
      disabled: computed(() => !(form.isModified.value || isNew.value) || !form.formMeta.value.valid),
      clickHandler: apply,
    },
    {
      id: "reset",
      title: t("ORDERS.PAGES.DETAILS.TOOLBAR.RESET"),
      icon: "lucide-undo-2",
      permissions: "order:update",
      disabled: computed(() => !form.isModified.value),
      clickHandler: () => {
        const base = original();
        if (base) entity.value = clone(base);
        form.setBaseline();
      },
    },
    {
      id: "delete",
      title: t("ORDERS.PAGES.DETAILS.TOOLBAR.DELETE"),
      icon: "lucide-trash-2",
      permissions: "order:delete",
      isVisible: computed(() => !isNew.value),
      disabled: computed(() => order.locked.value),
      async clickHandler() {
        if (!(await showConfirmation(t("ORDERS.ALERTS.DELETE_OPERATION_CONFIRMATION", { name: operationType })))) return;
        const target = list();
        const index = target.findIndex((x) => x.id === entity.value.id);
        if (index >= 0) target.splice(index, 1);
        form.setBaseline();
        await closeSelf();
      },
    },
    {
      id: "cancel",
      title: t("ORDERS.PAGES.DETAILS.TOOLBAR.CANCEL_DOCUMENT"),
      icon: "lucide-ban",
      permissions: "order:update",
      isVisible: computed(() => !isNew.value),
      disabled: computed(() => order.locked.value || !canCancel({ operationType, ...entity.value })),
      async clickHandler() {
        const reason = await askCancelReason();
        if (reason === false) return;
        applyCancel(entity.value, reason);
        await apply();
      },
    },
  ];

  // children (address, properties, discounts, transactions) work on this copy
  exposeToChildren({
    getEntity: () => entity.value,
  });

  return { kind, order, entity, isNew, locked, withPrices, loading, form, toolbar, operationType };
}
