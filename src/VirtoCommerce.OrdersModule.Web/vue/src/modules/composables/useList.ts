import { ref, type Ref } from "vue";
import { useApiClient, useAsync, useLatestRequest, useLoading } from "@vc-shell/framework";
import {
  OrderModuleClient,
  type CustomerOrder,
  type CustomerOrderSearchCriteria,
} from "../../api_client/virtocommerce.orders";

export type OrdersListQuery = CustomerOrderSearchCriteria & {
  /** use the search index (when enabled), as the AngularJS list does for keyword / filter searches */
  indexed?: boolean;
};

export default function useOrdersList() {
  const { getApiClient } = useApiClient(OrderModuleClient);
  const data: Ref<CustomerOrder[]> = ref([]);
  const totalCount = ref(0);
  const search = useLatestRequest();

  const { loading: itemsLoading, action: getItems } = useAsync<OrdersListQuery>(async (query) => {
    const client = await getApiClient();
    const { indexed, ...criteria } = query ?? {};
    const body = { responseGroup: "WithPrices", skip: 0, take: 20, ...criteria };
    const result = await search.latest(indexed ? client.searchCustomerOrderIndexed(body) : client.searchCustomerOrder(body));
    if (!result) return; // a newer search superseded this one
    data.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  });

  const { loading: deleteLoading, action: removeItems } = useAsync<string[]>(async (ids) => {
    const client = await getApiClient();
    await client.deleteOrdersByIds(ids ?? []);
  });

  const loading = useLoading(itemsLoading, deleteLoading);

  return { data, loading, totalCount, getItems, removeItems };
}
