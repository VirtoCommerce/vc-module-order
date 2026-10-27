import { defineExtensionPoint, type IBladeToolbar } from "@vc-shell/framework";
import { computed } from "vue";
import { useI18n } from "vue-i18n";

/**
 * A command other modules add to the toolbar of an Orders blade through its extension point
 * ("orders-list-toolbar", "orders-details-toolbar", "orders-line-item-toolbar"; each blade documents its ctx):
 *
 *   useExtensionPoint("orders-details-toolbar").add({
 *     id: "my-command",
 *     priority: 10,                               // order among the added commands
 *     component: markRaw({ render: () => null }), // required by the framework type, not rendered
 *     meta: {
 *       title: "MYMODULE.COMMAND",                // i18n key or text
 *       icon: "lucide-plus",
 *       permissions: "order:update",              // optional, string or string[] (any of)
 *       isVisible: (ctx) => true,                 // optional
 *       disabled: (ctx) => ctx.locked,            // optional
 *       onClick: (ctx) => ctx.openBlade({ name: "MyBlade", options: { orderId: ctx.order.id } }),
 *     } satisfies OrdersDetailsToolbarCommand,
 *   });
 */
export interface ToolbarCommand<Ctx> {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: Ctx) => boolean;
  disabled?: (ctx: Ctx) => boolean;
  onClick: (ctx: Ctx) => unknown;
}

/** Declares the blade's extension point; its commands as toolbar items, for the blade's context. */
export function useToolbarExtensions<Ctx>(name: string, description: string, context: () => Ctx) {
  const { t } = useI18n({ useScope: "global" });
  const { components } = defineExtensionPoint<ToolbarCommand<Ctx>>(name, { description });
  return computed<IBladeToolbar[]>(() => {
    const ctx = context();
    return components.value
      .filter((x) => x.meta && (!x.meta.isVisible || x.meta.isVisible(ctx)))
      .map((x) => ({
        id: x.id,
        title: t(x.meta!.title),
        icon: x.meta!.icon,
        permissions: x.meta!.permissions,
        disabled: !!x.meta!.disabled?.(ctx),
        // the context as it is when clicked
        clickHandler: () => x.meta!.onClick(context()),
      }));
  });
}
