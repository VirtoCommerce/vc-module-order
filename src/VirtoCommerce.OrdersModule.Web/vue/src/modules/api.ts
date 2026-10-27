// Small same-origin client for the sibling-module endpoints the Orders blades read
// (stores, members, payment/shipping methods, fulfillment centers, countries)
// and for the Orders endpoints whose generated client drops the response body (capture/refund, searchEnabled).
// ponytail: hand-written instead of generating five more module clients; generate them if these grow.

export interface SearchResult<T> {
  results?: T[];
  totalCount?: number;
}

export interface NamedEntity {
  id?: string;
  name?: string;
  code?: string;
  logoUrl?: string;
  description?: string;
  paymentMethodGroupType?: string;
  securityAccounts?: { id?: string }[];
  memberType?: string;
  regions?: NamedEntity[];
}

export interface PaymentFlowResult {
  succeeded?: boolean;
  errorMessage?: string;
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method,
    credentials: "same-origin",
    headers: body === undefined ? { Accept: "application/json" } : { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const text = await response.text();
  if (!response.ok) {
    throw new Error(text || `${response.status} ${response.statusText}`);
  }
  return (text ? JSON.parse(text) : undefined) as T;
}

/** VcSelect async options: (keyword, skip, ids) → { results, totalCount } */
export type OptionsLoader = (keyword?: string, skip?: number, ids?: string[]) => Promise<SearchResult<NamedEntity>>;

/** Members of a type (Contact, Organization, Employee...), or of any type */
export const searchMembers =
  (memberType?: string): OptionsLoader =>
  (keyword, skip, ids) =>
    request("POST", "/api/members/search", {
      memberType,
      deepSearch: true,
      sort: "name",
      keyword: ids?.length ? undefined : keyword,
      objectIds: ids?.length ? ids : undefined,
      skip: skip ?? 0,
      take: 20,
    });

export const searchAnyMember = searchMembers();
export const searchEmployees = searchMembers("Employee");

/** The name of the picked employee: the operations keep it denormalized next to the id, like the AngularJS selector */
export const employeeName = async (id: unknown) =>
  id ? (await searchEmployees(undefined, 0, [String(id)])).results?.[0]?.name : undefined;

export const getMember = (id: string) => request<NamedEntity>("GET", `/api/members/${encodeURIComponent(id)}`);
export const getMemberByUserId = (userId: string) =>
  request<NamedEntity>("GET", `/api/members/accounts/${encodeURIComponent(userId)}`);

export const searchStores = () => request<SearchResult<NamedEntity>>("POST", "/api/stores/search", { take: 1000 });

export const searchPaymentMethods = (storeId?: string) =>
  request<SearchResult<NamedEntity>>("POST", "/api/payment/search", { storeId, take: 100 });

export const searchShippingMethods = (storeId?: string) =>
  request<SearchResult<NamedEntity>>("POST", "/api/shipping/search", { storeId, take: 100 });

export const searchFulfillmentCenters = () =>
  request<SearchResult<NamedEntity>>("POST", "/api/inventory/fulfillmentcenters/search", { take: 100 });

/**
 * A setting's allowed values (else its value / default), as the AngularJS settings.getValues read them: this endpoint
 * needs only a signed-in user, not platform:setting:access. A failed lookup gives [] so it never blocks a blade.
 */
export const getSettingValues = (name: string) =>
  request<unknown[] | null>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`).then(
    (values) => (values ?? []).map(String),
    () => [] as string[],
  );

// static: fetched once (a failed lookup is retried on the next call)
let countries: Promise<NamedEntity[]> | undefined;
export const getCountries = () =>
  (countries ??= request<NamedEntity[]>("GET", "/api/platform/common/countries").catch((e) => {
    countries = undefined;
    throw e;
  }));
export const getRegions = (countryCode: string) =>
  request<NamedEntity[]>("GET", `/api/platform/common/countries/${encodeURIComponent(countryCode)}/regions`);

export const capturePayment = (body: Record<string, unknown>) =>
  request<PaymentFlowResult>("POST", "/api/order/payments/payment/capture", body);
export const refundPayment = (body: Record<string, unknown>) =>
  request<PaymentFlowResult>("POST", "/api/order/payments/payment/refund", body);

export const indexedSearchEnabled = () =>
  request<{ result?: boolean }>("GET", "/api/order/customerOrders/indexed/searchEnabled").then((x) => !!x?.result);
