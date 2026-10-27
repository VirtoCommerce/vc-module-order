import { markRaw } from "vue";
import { usePopup } from "@vc-shell/framework";
import CancelDialog from "../components/CancelDialog.vue";

/** Opens the cancel-document dialog; resolves the entered reason, or false when dismissed. */
export function useCancelPrompt() {
  let resolve: ((value: string | false) => void) | undefined;
  const settle = (value: string | false) => {
    resolve?.(value);
    resolve = undefined;
    popup.close();
  };
  const popup = usePopup({
    component: markRaw(CancelDialog),
    emits: { onConfirm: (reason: string) => settle(reason), onClose: () => settle(false) },
  });
  return () =>
    new Promise<string | false>((r) => {
      resolve = r;
      popup.open();
    });
}
