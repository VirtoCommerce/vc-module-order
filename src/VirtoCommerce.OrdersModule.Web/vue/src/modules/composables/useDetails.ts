import { ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";
import { OrderModuleClient, type CustomerOrder } from "../../api_client/virtocommerce.orders";
import { clearEmptyDates } from "../utils/operations";

export default function useOrdersDetails() {
  const { getApiClient } = useApiClient(OrderModuleClient);

  const item = ref<CustomerOrder>({});

  const { loading: itemLoading, action: fetchItem } = useAsync<string>(async (id) => {
    const client = await getApiClient();
    item.value = await client.getById(id!);
  });

  const { loading: saveLoading, action: saveAction } = useAsync(async () => {
    const client = await getApiClient();
    await client.updateOrder(item.value);
    item.value = await client.getById(item.value.id!);
  });

  // Recalculates totals server-side without saving (same as the AngularJS blade does on item edits)
  const { loading: recalcLoading, action: recalculate } = useAsync(async () => {
    const client = await getApiClient();
    const result = await client.calculateTotals(item.value);
    clearEmptyDates(result);
    item.value = result;
  });

  const { loading: deleteLoading, action: deleteAction } = useAsync(async () => {
    const client = await getApiClient();
    await client.deleteOrdersByIds([item.value.id!]);
  });

  const loading = useLoading(itemLoading, saveLoading, recalcLoading, deleteLoading);

  return { item, loading, getItem: fetchItem, saveItem: saveAction, recalculate, deleteItem: deleteAction };
}
