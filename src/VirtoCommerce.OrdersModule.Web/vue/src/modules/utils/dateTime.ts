// Dates with their time for Created / Modified (VCST-6559). The Admin Shell formats them in the user's regional format
// and time zone ($regional, see the shell's docs/plugins.md "Dates and numbers"); another vc-shell host falls back to
// the browser's. A stopgap until vc-shell has date-and-time field and column types (VCST-6956).
import { getCurrentInstance } from "vue";

type DateValue = Date | string | null | undefined;

interface RegionalSettings {
  formatDate: (value: DateValue, options?: Intl.DateTimeFormatOptions) => string;
  formatDateTime: (value: DateValue, options?: Intl.DateTimeFormatOptions) => string;
}

const DATE: Intl.DateTimeFormatOptions = { dateStyle: "medium" };
const TIME: Intl.DateTimeFormatOptions = { timeStyle: "short" };
const DATE_TIME: Intl.DateTimeFormatOptions = { dateStyle: "medium", timeStyle: "short" };

function browserFormat(value: DateValue, options: Intl.DateTimeFormatOptions) {
  if (!value) return "";
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? "" : new Intl.DateTimeFormat(undefined, options).format(date);
}

/** Formatters for a component: the host's regional settings when it has them, else the browser's */
export function useDateTime() {
  const regional = getCurrentInstance()?.appContext.config.globalProperties.$regional as RegionalSettings | undefined;
  const format = (value: DateValue, options: Intl.DateTimeFormatOptions) =>
    regional ? regional.formatDateTime(value, options) : browserFormat(value, options);
  return {
    /** "Nov 19, 2026, 10:18 AM" */
    dateTime: (value: DateValue) => format(value, DATE_TIME),
    /** "Nov 19, 2026" */
    date: (value: DateValue) => format(value, DATE),
    /** "10:18 AM" */
    time: (value: DateValue) => format(value, TIME),
  };
}
