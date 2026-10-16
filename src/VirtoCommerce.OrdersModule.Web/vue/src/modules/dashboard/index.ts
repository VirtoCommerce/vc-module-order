// Orders widgets on the Admin Shell dashboard. Every widget declares a position (the grid only honours
// declared positions when all widgets have one); the rows follow the design's Dashboard.jsx.
import { registerDashboardWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import FulfillmentWidget from "./FulfillmentWidget.vue";
import OrdersToProcessWidget from "./OrdersToProcessWidget.vue";
import OutstandingPaymentsWidget from "./OutstandingPaymentsWidget.vue";
import RecentOrdersWidget from "./RecentOrdersWidget.vue";
import SalesTrendsWidget from "./SalesTrendsWidget.vue";
import TotalSalesWidget from "./TotalSalesWidget.vue";

const VIEW_STATISTICS = "order:dashboardstatistics:view";

registerDashboardWidget({
  id: "orders-total-sales",
  name: "Total sales",
  component: markRaw(TotalSalesWidget),
  size: { width: 4, height: 5 },
  position: { x: 0, y: 5 },
  permissions: [VIEW_STATISTICS],
});

registerDashboardWidget({
  id: "orders-sales-trends",
  name: "Sales trends",
  component: markRaw(SalesTrendsWidget),
  size: { width: 8, height: 5 },
  position: { x: 4, y: 5 },
  permissions: [VIEW_STATISTICS],
});

registerDashboardWidget({
  id: "orders-recent",
  name: "Recent orders",
  component: markRaw(RecentOrdersWidget),
  size: { width: 8, height: 6 },
  position: { x: 0, y: 10 },
  permissions: ["order:read"],
});

registerDashboardWidget({
  id: "orders-to-process",
  name: "Orders to process",
  component: markRaw(OrdersToProcessWidget),
  size: { width: 4, height: 6 },
  position: { x: 8, y: 16 },
  permissions: ["order:read"],
});

registerDashboardWidget({
  id: "orders-outstanding-payments",
  name: "Outstanding payments",
  component: markRaw(OutstandingPaymentsWidget),
  size: { width: 5, height: 5 },
  position: { x: 0, y: 22 },
  permissions: ["order:read"],
});

registerDashboardWidget({
  id: "orders-fulfillment",
  name: "Fulfillment",
  component: markRaw(FulfillmentWidget),
  size: { width: 3, height: 5 },
  position: { x: 9, y: 22 },
  permissions: ["order:read"],
});
