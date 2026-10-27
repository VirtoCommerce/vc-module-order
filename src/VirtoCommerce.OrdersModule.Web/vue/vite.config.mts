import path from "node:path";
import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

// Builds the Orders admin UI as a Module Federation remote of the Admin Shell host.
// Output: <module>/plugins/admin-shell/ (remoteEntry.js + public/plugin.json with the menu contribution),
// served by the platform at /modules/$(VirtoCommerce.Orders)/plugins/admin-shell/.
const base = "/modules/$(VirtoCommerce.Orders)/plugins/admin-shell/";

export default getDynamicModuleConfiguration({
  appId: "admin-shell",
  moduleRoot: path.resolve(__dirname, ".."),
  entry: "./src/modules/index.ts",
  remoteName: "orders_admin",
  base,
});
