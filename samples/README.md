# Order Extension Sample Module

This sample demonstrates the Virto Commerce Extensibility Framework in action by extending the native Order Module. It includes various scenarios to showcase extensibility features for REST API contracts, database contracts, and Admin Back Office extension points.

**Note:** This sample is intended for demo purposes, training sessions, etc. We recommend following a 3-tier architecture (Core, Data, Web, or XApi) for custom modules, as well as for Virto Commerce.

**Note:** The sample doesn't support the `vc-build` command.

## Extension Scenarios

1. **Extend CustomerOrder with Invoices and NewField:** Demonstrates how to extend REST API contracts.
2. **Extend CustomerOrder2Entity and Order2DbContext:** Demonstrates how to extend database contracts.
3. **Extend LineItem2Entity with OuterId custom property.**
4. **Override implementation with AbstractTypeFactory and DI:** Demonstrates initialization with dependency injection.
5. **Line Item Validator:** Demonstrates how to create a custom line item validator.
6. **Create CustomOrderAuthorizationHandler.**
7. **Admin Back Office Extension Points:**
  * Register Invoice Order Document with the ability to create, view, edit, and delete.
  * Extend CustomerOrder blade with NewField via Virto Commerce MetaFields.
  * Extend Custom Order Grid View with NewField column.

## Getting Started

### Prerequisites

1. Download and run the latest Virto Commerce Edge Release with ECommerce Bundle.

### Installation

1. Download and open the `VirtoCommerce.OrdersModule` solution in Visual Studio 2022.
2. Compile `VirtoCommerce.OrdersModule2.Web`.
3. Build Admin UI code by running the following commands in the `VirtoCommerce.OrdersModule2.Web` folder:

```cmd
npm ci
```

```cmd
npm run webpack:dev
```

4. Rename `_module.manifest` to `module.manifest`.
5. Install the module by creating a symbolic link in the Virto Commerce Modules folder using the following command (change the path):

```cmd
mklink /D "c:\vc-platform-3-demo\platform\modules\Order.Ext" "c:\Projects\git\VirtoCommerce\vc-module-order\samples\VirtoCommerce.OrdersModule2.Web"
```

6. Run Virto Commerce Platform and enjoy the Virto Commerce Extensibility Framework.

## Screenshots

![image](https://github.com/VirtoCommerce/vc-module-order/assets/7639413/e9c320e7-611b-47ad-9df5-e08e4862c7e1)
![image](https://github.com/VirtoCommerce/vc-module-order/assets/7639413/ec24497d-62e4-4622-ace3-60f0e220a0d2)
![image](https://github.com/VirtoCommerce/vc-module-order/assets/7639413/03338cd7-02ab-4473-bde2-5712a09c4b16)
![image](https://github.com/VirtoCommerce/vc-module-order/assets/7639413/70dbd44c-19d1-4db2-9842-802a0b4471a7)
![image](https://github.com/VirtoCommerce/vc-module-order/assets/7639413/fe10182c-b304-4dac-a492-57fa44ba3167)




