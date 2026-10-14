import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import MemberOrdersWidget from "./widgets/MemberOrdersWidget.vue";
// registers the dashboard widgets (must run before the host builds the dashboard)
import "./dashboard";

// the AngularJS customerOrdersWidget on customerDetail1 / organizationDetail1: orders of the contact or organization
registerExternalWidget({
  id: "OrdersMemberOrders",
  title: "Orders",
  component: markRaw(MemberOrdersWidget),
  targetBlades: ["CustomerContactDetails", "CustomerOrganizationDetails"],
});

export default defineAppModule({
  blades,
  locales,
});
