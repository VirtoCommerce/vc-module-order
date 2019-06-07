# Orders Module Documents

Order documents are created by the system to store additional order details. There are two types of documents that are provided ‘out of the box’:

1. **Payment document**
1. **Shipment document**

The user can access the order documents by selecting a specific order. Order documents are displayed on order details screen.
![Fig.Customer Orders](/docs/media/screen-order-documents1.png)

## Order Payment Document

The user can view the payment details by clicking on the Order Payment Document.

![Fig.Customer Orders](/docs/media/screen-payment-document-details.png)

The  Payment document contains the following fields:

1. **Payment number** generated automatically by the system.
     1. The order payment number template can be specified by the admin user as follows:
         1. The user navigates to Settings->Orders->General;
         1. The system opens the General Settings screen;
         1. The user specifies the Order payment number template.
![Fig.Customer Orders](/docs/media/screen-order-payment-number-template.png)
1. **From** label displays the order creation date.
1. **Payment** fees input field with currency as a label.
1. **Payment fees incl. taxes** input field with currency as a label.
1. **Status** drop down.
1. **Payment purpose** input field.
1. **Amount** input field.

The order payment document screen contains the following **widgets**:

![Fig.Customer Orders](/docs/media/screen-payment-document-widgets.png)

1. **Comments** – contains all comments related to order payments.
1. **Payment address** widget displays the billing address details:
     1. Address Name;
     1. Address type: Shipping/ Billing and Shipping;
     1. First Name;
     1. Last Name;
     1. Currency;
     1. Region;
     1. City;
     1. Address 1;
     1. Address 2;
     1. Zip code;
     1. Email;
     1. Phone.
1. **Transactions** widget displays the list of all payment gateway transactions.
1. **Totals** widget displays the total payment details:
     1. Payment price;
     1. Discount total;
     1. Tax total;
     1. Total.
1. **Changes** widget displays the payment changes history (logs).
1. **Dynamic properties** widget.

## Order Shipment Document

The user can access the Shipment document details by clicking  the ‘Shipment’ document as displayed on the screenshot bellow.

The Order shipment document screen displays the following fields:

![Fig.Customer Orders](/docs/media/screen-shipment-details.png)

1. **Order Shipment number** generated automatically by the system. The order shipment number template can be specified by user:
     1. The user navigates to settings->Orders->General;
     1. The system opens the General Settings screen;
     1. The user specifies the Order shipment number template.
![Fig.Customer Orders](/docs/media/screen-order-shipment-number-template.png)
1. **From** (label) displays the order creation date.
1. **Status** drop down displays the shipment status.
1. **Assigned to** drop down displays the name of the person responsible for the shipment processing.
1. **Shipment amount** input field.
1. **Shipment amount with tax**
1. **Fulfillment center** drop down.

The order shipment document screen contains the following **widgets**:

![Fig.Customer Orders](/docs/media/screen-order-shipment-document-widgets.png)

1. Shipment items widget displays the list of items for shipping.
1. Changes widget displays the shipment changes history (logs).
1. Comments widget displays the comments related to the shipment.
1. Dynamic properties widget.
1. Delivery address widget.
1. Totals widget:
     1. Shipping price;
     1. Discount total;
     1. Tax total;
     1. Total.
