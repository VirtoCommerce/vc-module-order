// Order statistics for the dashboard widgets (GET api/order/dashboardStatistics).
// Requests are cached at module scope: the dashboard page unmounts whenever a blade opens,
// and coming back must not refetch a year of monthly buckets.
import type { DashboardMoney, DashboardStatisticsResult } from "../../api_client/virtocommerce.orders";

const DAY = 86_400_000;
const cache = new Map<string, Promise<DashboardStatisticsResult>>();

// UTC time without "Z": ASP.NET binds it unchanged and compares it with the UTC CreatedDate,
// where an ISO string with "Z" would be shifted into the server's time zone
const stamp = (date: Date) => date.toISOString().slice(0, 19);

/** Statistics for orders created in [start, end). */
export function getStatistics(start: Date, end: Date): Promise<DashboardStatisticsResult> {
  // the endpoint adds 2 days to `end` ("Hack" in OrderModuleController), so ask for 2 days (and a second) less
  const query = `start=${stamp(start)}&end=${stamp(new Date(end.getTime() - 2 * DAY - 1000))}`;
  let request = cache.get(query);
  if (!request) {
    request = fetch(`/api/order/dashboardStatistics?${query}`).then((response) => {
      if (!response.ok) throw new Error(`dashboardStatistics: HTTP ${response.status}`);
      return response.json() as Promise<DashboardStatisticsResult>;
    });
    // a failed request is retried on the next mount
    request.catch(() => cache.delete(query));
    cache.set(query, request);
  }
  return request;
}

/** Start of tomorrow (UTC), so periods ending "now" keep the same cache key all day. */
export function endOfToday(): Date {
  const now = new Date();
  return new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() + 1));
}

export const daysBefore = (date: Date, days: number) => new Date(date.getTime() - days * DAY);

export interface Bucket {
  start: Date;
  end: Date;
  label: string;
}

/** The last `count` calendar months (UTC), oldest first, the current month last. */
export function monthBuckets(count: number): Bucket[] {
  const now = new Date();
  return Array.from({ length: count }, (_, i) => {
    const month = now.getUTCMonth() - (count - 1 - i);
    const start = new Date(Date.UTC(now.getUTCFullYear(), month, 1));
    return {
      start,
      end: new Date(Date.UTC(now.getUTCFullYear(), month + 1, 1)),
      label: start.toLocaleString(undefined, { month: "short", timeZone: "UTC" }),
    };
  });
}

/** The last `count` calendar quarters (UTC), oldest first. */
export function quarterBuckets(count: number): Bucket[] {
  const now = new Date();
  const current = Math.floor(now.getUTCMonth() / 3);
  return Array.from({ length: count }, (_, i) => {
    const firstMonth = (current - (count - 1 - i)) * 3;
    const start = new Date(Date.UTC(now.getUTCFullYear(), firstMonth, 1));
    return {
      start,
      end: new Date(Date.UTC(now.getUTCFullYear(), firstMonth + 3, 1)),
      label: `Q${Math.floor(start.getUTCMonth() / 3) + 1} '${String(start.getUTCFullYear()).slice(2)}`,
    };
  });
}

/** Amount in one currency; the endpoint returns null collections when statistics are disabled. */
export const amountIn = (money: DashboardMoney[] | null | undefined, currency: string) =>
  money?.find((x) => x.currency === currency)?.amount ?? 0;

/** The currency with the most revenue across the results (revenue is reported per currency). */
export function mainCurrency(results: DashboardStatisticsResult[]): string {
  const totals = new Map<string, number>();
  for (const money of results.flatMap((r) => r.revenue ?? [])) {
    if (money.currency) totals.set(money.currency, (totals.get(money.currency) ?? 0) + (money.amount ?? 0));
  }
  return [...totals].sort((a, b) => b[1] - a[1])[0]?.[0] ?? "USD";
}

/**
 * A number read left to right in a right-to-left UI too: "+$30,428" has no letters, so it would take the page's
 * direction and show its sign at the wrong end (Unicode isolates, invisible in LTR).
 */
export const ltr = (text: string) => `⁦${text}⁩`;

/** Whole units from 1,000 up (KPIs, charts) unless `exact` (tables, where a column must line up). */
export function formatMoney(amount: number, currency: string, signed = false, exact = false): string {
  return ltr(new Intl.NumberFormat(undefined, {
    style: "currency",
    currency,
    minimumFractionDigits: exact ? 2 : undefined,
    maximumFractionDigits: exact || Math.abs(amount) < 1000 ? 2 : 0,
    signDisplay: signed ? "exceptZero" : "auto",
  }).format(amount));
}

/** The date part of a timestamp, in the user's locale (table cells). */
export const formatDate = (value?: Date | string | null) => (value ? new Date(value).toLocaleDateString() : "");
