import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw, type App } from "vue";
import MemberOrdersWidget from "./widgets/MemberOrdersWidget.vue";
import { registerOrderSearch } from "./search";
// registers the dashboard widgets (must run before the host builds the dashboard)
import "./dashboard";

// the AngularJS customerOrdersWidget on customerDetail1 / organizationDetail1: orders of the contact or organization
registerExternalWidget({
  id: "OrdersMemberOrders",
  title: "Orders",
  component: markRaw(MemberOrdersWidget),
  targetBlades: ["CustomerContactDetails", "CustomerOrganizationDetails"],
});

const appModule = defineAppModule({
  blades,
  locales,
});

export default {
  install(app: App) {
    appModule.install(app);
    // orders in the Admin Shell's global search
    registerOrderSearch(app);
  },
};
