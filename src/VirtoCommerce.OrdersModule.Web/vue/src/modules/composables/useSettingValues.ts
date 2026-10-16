import { ref } from "vue";
import { getSettingValues } from "../api";

/** Allowed values of a dictionary setting (Order.Status, PaymentIn.Status...) as select options. */
export function useSettingValues(name: string) {
  const values = ref<{ id: string; title: string }[]>([]);
  getSettingValues(name).then((x) => (values.value = x.map((id) => ({ id, title: id }))));
  return values;
}
