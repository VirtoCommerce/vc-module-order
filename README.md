# VirtoCommerce.Order
VirtoCommerce.Order module represents customer Order management system.
Key features:
* payment and shipment management

![Order UI](https://cloud.githubusercontent.com/assets/5801549/15569048/854c646c-232f-11e6-9819-7230045cad0c.png)

# Documentation
User guide: <a href="http://docs.virtocommerce.com/x/ygHr" target="_blank">Order Management</a>

Developer guide:

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Order module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-order/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
* **Order.Status** - customer order statuses (Processing, Cancelled, Completed, etc.);
* **Shipment.Status** - shipment statuses (New, PickPack, ReadyToSend, etc.);
* **PaymentIn.Status** - incoming payment statuses (New, Pending, Authorized, etc.);
* **Order.CustomerOrderNewNumberTemplate** - template for new Order number generation;
* **Order.ShipmentNewNumberTemplate** - template for new shipment number generation;
* **Order.PaymentInNewNumberTemplate** - template for new incoming payment number generation.

# Available resources
* Module related service implementations as a <a href="https://www.nuget.org/packages/VirtoCommerce.OrderModule.Data" target="_blank">NuGet package</a>
* API client as a <a href="" target="_blank">NuGet package **place url here**</a>
* API client documentation http://demo.virtocommerce.com/admin/docs/ui/index#!/Order_module

# License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
