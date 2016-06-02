using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using VirtoCommerce.OrderModule.Client.Client;
using VirtoCommerce.OrderModule.Client.Model;

namespace VirtoCommerce.OrderModule.Client.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IVirtoCommerceOrdersApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>CustomerOrder</returns>
        CustomerOrder OrderModuleCreateOrder(CustomerOrder customerOrder);

        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        ApiResponse<CustomerOrder> OrderModuleCreateOrderWithHttpInfo(CustomerOrder customerOrder);
        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>CustomerOrder</returns>
        CustomerOrder OrderModuleCreateOrderFromCart(string id);

        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        ApiResponse<CustomerOrder> OrderModuleCreateOrderFromCartWithHttpInfo(string id);
        /// <summary>
        /// Delete a concrete customer order operation (document)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns></returns>
        void OrderModuleDelete(string id, string operationId);

        /// <summary>
        /// Delete a concrete customer order operation (document)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> OrderModuleDeleteWithHttpInfo(string id, string operationId);
        /// <summary>
        /// Delete a whole customer orders
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns></returns>
        void OrderModuleDeleteOrdersByIds(List<string> ids);

        /// <summary>
        /// Delete a whole customer orders
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> OrderModuleDeleteOrdersByIdsWithHttpInfo(List<string> ids);
        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>CustomerOrder</returns>
        CustomerOrder OrderModuleGetById(string id);

        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        ApiResponse<CustomerOrder> OrderModuleGetByIdWithHttpInfo(string id);
        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>CustomerOrder</returns>
        CustomerOrder OrderModuleGetByNumber(string number);

        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        ApiResponse<CustomerOrder> OrderModuleGetByNumberWithHttpInfo(string number);
        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>DashboardStatisticsResult</returns>
        DashboardStatisticsResult OrderModuleGetDashboardStatistics(DateTime? start = null, DateTime? end = null);

        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>ApiResponse of DashboardStatisticsResult</returns>
        ApiResponse<DashboardStatisticsResult> OrderModuleGetDashboardStatisticsWithHttpInfo(DateTime? start = null, DateTime? end = null);
        /// <summary>
        /// Get new payment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new payment  document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>PaymentIn</returns>
        PaymentIn OrderModuleGetNewPayment(string id);

        /// <summary>
        /// Get new payment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new payment  document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of PaymentIn</returns>
        ApiResponse<PaymentIn> OrderModuleGetNewPaymentWithHttpInfo(string id);
        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new shipment document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Shipment</returns>
        Shipment OrderModuleGetNewShipment(string id);

        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new shipment document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of Shipment</returns>
        ApiResponse<Shipment> OrderModuleGetNewShipmentWithHttpInfo(string id);
        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>
        /// Used in storefront checkout or manual order payment registration
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>ProcessPaymentResult</returns>
        ProcessPaymentResult OrderModuleProcessOrderPayments(string orderId, string paymentId, BankCardInfo bankCardInfo = null);

        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>
        /// Used in storefront checkout or manual order payment registration
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>ApiResponse of ProcessPaymentResult</returns>
        ApiResponse<ProcessPaymentResult> OrderModuleProcessOrderPaymentsWithHttpInfo(string orderId, string paymentId, BankCardInfo bankCardInfo = null);
        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>SearchResult</returns>
        SearchResult OrderModuleSearch(SearchCriteria criteria);

        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>ApiResponse of SearchResult</returns>
        ApiResponse<SearchResult> OrderModuleSearchWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Update a existing customer order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns></returns>
        void OrderModuleUpdate(CustomerOrder customerOrder);

        /// <summary>
        /// Update a existing customer order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> OrderModuleUpdateWithHttpInfo(CustomerOrder customerOrder);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of CustomerOrder</returns>
        System.Threading.Tasks.Task<CustomerOrder> OrderModuleCreateOrderAsync(CustomerOrder customerOrder);

        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleCreateOrderAsyncWithHttpInfo(CustomerOrder customerOrder);
        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>Task of CustomerOrder</returns>
        System.Threading.Tasks.Task<CustomerOrder> OrderModuleCreateOrderFromCartAsync(string id);

        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleCreateOrderFromCartAsyncWithHttpInfo(string id);
        /// <summary>
        /// Delete a concrete customer order operation (document)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task OrderModuleDeleteAsync(string id, string operationId);

        /// <summary>
        /// Delete a concrete customer order operation (document)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleDeleteAsyncWithHttpInfo(string id, string operationId);
        /// <summary>
        /// Delete a whole customer orders
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task OrderModuleDeleteOrdersByIdsAsync(List<string> ids);

        /// <summary>
        /// Delete a whole customer orders
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleDeleteOrdersByIdsAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of CustomerOrder</returns>
        System.Threading.Tasks.Task<CustomerOrder> OrderModuleGetByIdAsync(string id);

        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleGetByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>Task of CustomerOrder</returns>
        System.Threading.Tasks.Task<CustomerOrder> OrderModuleGetByNumberAsync(string number);

        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>
        /// Return a single customer order with all nested documents or null if order was not found
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleGetByNumberAsyncWithHttpInfo(string number);
        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>Task of DashboardStatisticsResult</returns>
        System.Threading.Tasks.Task<DashboardStatisticsResult> OrderModuleGetDashboardStatisticsAsync(DateTime? start = null, DateTime? end = null);

        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>Task of ApiResponse (DashboardStatisticsResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<DashboardStatisticsResult>> OrderModuleGetDashboardStatisticsAsyncWithHttpInfo(DateTime? start = null, DateTime? end = null);
        /// <summary>
        /// Get new payment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new payment  document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of PaymentIn</returns>
        System.Threading.Tasks.Task<PaymentIn> OrderModuleGetNewPaymentAsync(string id);

        /// <summary>
        /// Get new payment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new payment  document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (PaymentIn)</returns>
        System.Threading.Tasks.Task<ApiResponse<PaymentIn>> OrderModuleGetNewPaymentAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new shipment document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of Shipment</returns>
        System.Threading.Tasks.Task<Shipment> OrderModuleGetNewShipmentAsync(string id);

        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>
        /// Return new shipment document with populates all required properties.
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (Shipment)</returns>
        System.Threading.Tasks.Task<ApiResponse<Shipment>> OrderModuleGetNewShipmentAsyncWithHttpInfo(string id);
        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>
        /// Used in storefront checkout or manual order payment registration
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>Task of ProcessPaymentResult</returns>
        System.Threading.Tasks.Task<ProcessPaymentResult> OrderModuleProcessOrderPaymentsAsync(string orderId, string paymentId, BankCardInfo bankCardInfo = null);

        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>
        /// Used in storefront checkout or manual order payment registration
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>Task of ApiResponse (ProcessPaymentResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<ProcessPaymentResult>> OrderModuleProcessOrderPaymentsAsyncWithHttpInfo(string orderId, string paymentId, BankCardInfo bankCardInfo = null);
        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>Task of SearchResult</returns>
        System.Threading.Tasks.Task<SearchResult> OrderModuleSearchAsync(SearchCriteria criteria);

        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>Task of ApiResponse (SearchResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<SearchResult>> OrderModuleSearchAsyncWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Update a existing customer order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task OrderModuleUpdateAsync(CustomerOrder customerOrder);

        /// <summary>
        /// Update a existing customer order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleUpdateAsyncWithHttpInfo(CustomerOrder customerOrder);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class VirtoCommerceOrdersApi : IVirtoCommerceOrdersApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommerceOrdersApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="apiClient">An instance of ApiClient.</param>
        /// <returns></returns>
        public VirtoCommerceOrdersApi(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Configuration = apiClient.Configuration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the API client object
        /// </summary>
        /// <value>An instance of the ApiClient</value>
        public ApiClient ApiClient { get; set; }

        /// <summary>
        /// Add new customer order to system 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>CustomerOrder</returns>
        public CustomerOrder OrderModuleCreateOrder(CustomerOrder customerOrder)
        {
             ApiResponse<CustomerOrder> localVarResponse = OrderModuleCreateOrderWithHttpInfo(customerOrder);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Add new customer order to system 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        public ApiResponse<CustomerOrder> OrderModuleCreateOrderWithHttpInfo(CustomerOrder customerOrder)
        {
            // verify the required parameter 'customerOrder' is set
            if (customerOrder == null)
                throw new ApiException(400, "Missing required parameter 'customerOrder' when calling VirtoCommerceOrdersApi->OrderModuleCreateOrder");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (customerOrder.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(customerOrder); // http body (model) parameter
            }
            else
            {
                localVarPostBody = customerOrder; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrder: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrder: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }

        /// <summary>
        /// Add new customer order to system 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of CustomerOrder</returns>
        public async System.Threading.Tasks.Task<CustomerOrder> OrderModuleCreateOrderAsync(CustomerOrder customerOrder)
        {
             ApiResponse<CustomerOrder> localVarResponse = await OrderModuleCreateOrderAsyncWithHttpInfo(customerOrder);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Add new customer order to system 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleCreateOrderAsyncWithHttpInfo(CustomerOrder customerOrder)
        {
            // verify the required parameter 'customerOrder' is set
            if (customerOrder == null)
                throw new ApiException(400, "Missing required parameter 'customerOrder' when calling VirtoCommerceOrdersApi->OrderModuleCreateOrder");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (customerOrder.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(customerOrder); // http body (model) parameter
            }
            else
            {
                localVarPostBody = customerOrder; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrder: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrder: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }
        /// <summary>
        /// Create new customer order based on shopping cart. 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>CustomerOrder</returns>
        public CustomerOrder OrderModuleCreateOrderFromCart(string id)
        {
             ApiResponse<CustomerOrder> localVarResponse = OrderModuleCreateOrderFromCartWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create new customer order based on shopping cart. 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        public ApiResponse<CustomerOrder> OrderModuleCreateOrderFromCartWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleCreateOrderFromCart");

            var localVarPath = "/api/order/customerOrders/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrderFromCart: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrderFromCart: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }

        /// <summary>
        /// Create new customer order based on shopping cart. 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>Task of CustomerOrder</returns>
        public async System.Threading.Tasks.Task<CustomerOrder> OrderModuleCreateOrderFromCartAsync(string id)
        {
             ApiResponse<CustomerOrder> localVarResponse = await OrderModuleCreateOrderFromCartAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create new customer order based on shopping cart. 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">shopping cart id</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleCreateOrderFromCartAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleCreateOrderFromCart");

            var localVarPath = "/api/order/customerOrders/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrderFromCart: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleCreateOrderFromCart: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }
        /// <summary>
        /// Delete a concrete customer order operation (document) 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns></returns>
        public void OrderModuleDelete(string id, string operationId)
        {
             OrderModuleDeleteWithHttpInfo(id, operationId);
        }

        /// <summary>
        /// Delete a concrete customer order operation (document) 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> OrderModuleDeleteWithHttpInfo(string id, string operationId)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleDelete");
            // verify the required parameter 'operationId' is set
            if (operationId == null)
                throw new ApiException(400, "Missing required parameter 'operationId' when calling VirtoCommerceOrdersApi->OrderModuleDelete");

            var localVarPath = "/api/order/customerOrders/{id}/operations/{operationId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter
            if (operationId != null) localVarPathParams.Add("operationId", ApiClient.ParameterToString(operationId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete a concrete customer order operation (document) 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task OrderModuleDeleteAsync(string id, string operationId)
        {
             await OrderModuleDeleteAsyncWithHttpInfo(id, operationId);

        }

        /// <summary>
        /// Delete a concrete customer order operation (document) 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleDeleteAsyncWithHttpInfo(string id, string operationId)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleDelete");
            // verify the required parameter 'operationId' is set
            if (operationId == null)
                throw new ApiException(400, "Missing required parameter 'operationId' when calling VirtoCommerceOrdersApi->OrderModuleDelete");

            var localVarPath = "/api/order/customerOrders/{id}/operations/{operationId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter
            if (operationId != null) localVarPathParams.Add("operationId", ApiClient.ParameterToString(operationId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Delete a whole customer orders 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns></returns>
        public void OrderModuleDeleteOrdersByIds(List<string> ids)
        {
             OrderModuleDeleteOrdersByIdsWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete a whole customer orders 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> OrderModuleDeleteOrdersByIdsWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceOrdersApi->OrderModuleDeleteOrdersByIds");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDeleteOrdersByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDeleteOrdersByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete a whole customer orders 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task OrderModuleDeleteOrdersByIdsAsync(List<string> ids)
        {
             await OrderModuleDeleteOrdersByIdsAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete a whole customer orders 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">customer order ids for delete</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleDeleteOrdersByIdsAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceOrdersApi->OrderModuleDeleteOrdersByIds");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDeleteOrdersByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleDeleteOrdersByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Find customer order by id Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>CustomerOrder</returns>
        public CustomerOrder OrderModuleGetById(string id)
        {
             ApiResponse<CustomerOrder> localVarResponse = OrderModuleGetByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Find customer order by id Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        public ApiResponse<CustomerOrder> OrderModuleGetByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetById");

            var localVarPath = "/api/order/customerOrders/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }

        /// <summary>
        /// Find customer order by id Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of CustomerOrder</returns>
        public async System.Threading.Tasks.Task<CustomerOrder> OrderModuleGetByIdAsync(string id)
        {
             ApiResponse<CustomerOrder> localVarResponse = await OrderModuleGetByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Find customer order by id Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleGetByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetById");

            var localVarPath = "/api/order/customerOrders/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }
        /// <summary>
        /// Find customer order by number Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>CustomerOrder</returns>
        public CustomerOrder OrderModuleGetByNumber(string number)
        {
             ApiResponse<CustomerOrder> localVarResponse = OrderModuleGetByNumberWithHttpInfo(number);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Find customer order by number Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>ApiResponse of CustomerOrder</returns>
        public ApiResponse<CustomerOrder> OrderModuleGetByNumberWithHttpInfo(string number)
        {
            // verify the required parameter 'number' is set
            if (number == null)
                throw new ApiException(400, "Missing required parameter 'number' when calling VirtoCommerceOrdersApi->OrderModuleGetByNumber");

            var localVarPath = "/api/order/customerOrders/number/{number}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (number != null) localVarPathParams.Add("number", ApiClient.ParameterToString(number)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetByNumber: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetByNumber: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }

        /// <summary>
        /// Find customer order by number Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>Task of CustomerOrder</returns>
        public async System.Threading.Tasks.Task<CustomerOrder> OrderModuleGetByNumberAsync(string number)
        {
             ApiResponse<CustomerOrder> localVarResponse = await OrderModuleGetByNumberAsyncWithHttpInfo(number);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Find customer order by number Return a single customer order with all nested documents or null if order was not found
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="number">customer order number</param>
        /// <returns>Task of ApiResponse (CustomerOrder)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CustomerOrder>> OrderModuleGetByNumberAsyncWithHttpInfo(string number)
        {
            // verify the required parameter 'number' is set
            if (number == null)
                throw new ApiException(400, "Missing required parameter 'number' when calling VirtoCommerceOrdersApi->OrderModuleGetByNumber");

            var localVarPath = "/api/order/customerOrders/number/{number}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (number != null) localVarPathParams.Add("number", ApiClient.ParameterToString(number)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetByNumber: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetByNumber: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CustomerOrder>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CustomerOrder)ApiClient.Deserialize(localVarResponse, typeof(CustomerOrder)));
            
        }
        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>DashboardStatisticsResult</returns>
        public DashboardStatisticsResult OrderModuleGetDashboardStatistics(DateTime? start = null, DateTime? end = null)
        {
             ApiResponse<DashboardStatisticsResult> localVarResponse = OrderModuleGetDashboardStatisticsWithHttpInfo(start, end);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>ApiResponse of DashboardStatisticsResult</returns>
        public ApiResponse<DashboardStatisticsResult> OrderModuleGetDashboardStatisticsWithHttpInfo(DateTime? start = null, DateTime? end = null)
        {

            var localVarPath = "/api/order/dashboardStatistics";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (start != null) localVarQueryParams.Add("start", ApiClient.ParameterToString(start)); // query parameter
            if (end != null) localVarQueryParams.Add("end", ApiClient.ParameterToString(end)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetDashboardStatistics: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetDashboardStatistics: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<DashboardStatisticsResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (DashboardStatisticsResult)ApiClient.Deserialize(localVarResponse, typeof(DashboardStatisticsResult)));
            
        }

        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>Task of DashboardStatisticsResult</returns>
        public async System.Threading.Tasks.Task<DashboardStatisticsResult> OrderModuleGetDashboardStatisticsAsync(DateTime? start = null, DateTime? end = null)
        {
             ApiResponse<DashboardStatisticsResult> localVarResponse = await OrderModuleGetDashboardStatisticsAsyncWithHttpInfo(start, end);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get a some order statistic information for Commerce manager dashboard 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start">start interval date (optional)</param>
        /// <param name="end">end interval date (optional)</param>
        /// <returns>Task of ApiResponse (DashboardStatisticsResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<DashboardStatisticsResult>> OrderModuleGetDashboardStatisticsAsyncWithHttpInfo(DateTime? start = null, DateTime? end = null)
        {

            var localVarPath = "/api/order/dashboardStatistics";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (start != null) localVarQueryParams.Add("start", ApiClient.ParameterToString(start)); // query parameter
            if (end != null) localVarQueryParams.Add("end", ApiClient.ParameterToString(end)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetDashboardStatistics: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetDashboardStatistics: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<DashboardStatisticsResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (DashboardStatisticsResult)ApiClient.Deserialize(localVarResponse, typeof(DashboardStatisticsResult)));
            
        }
        /// <summary>
        /// Get new payment for specified customer order Return new payment  document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>PaymentIn</returns>
        public PaymentIn OrderModuleGetNewPayment(string id)
        {
             ApiResponse<PaymentIn> localVarResponse = OrderModuleGetNewPaymentWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get new payment for specified customer order Return new payment  document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of PaymentIn</returns>
        public ApiResponse<PaymentIn> OrderModuleGetNewPaymentWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetNewPayment");

            var localVarPath = "/api/order/customerOrders/{id}/payments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewPayment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewPayment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PaymentIn>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PaymentIn)ApiClient.Deserialize(localVarResponse, typeof(PaymentIn)));
            
        }

        /// <summary>
        /// Get new payment for specified customer order Return new payment  document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of PaymentIn</returns>
        public async System.Threading.Tasks.Task<PaymentIn> OrderModuleGetNewPaymentAsync(string id)
        {
             ApiResponse<PaymentIn> localVarResponse = await OrderModuleGetNewPaymentAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get new payment for specified customer order Return new payment  document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (PaymentIn)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<PaymentIn>> OrderModuleGetNewPaymentAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetNewPayment");

            var localVarPath = "/api/order/customerOrders/{id}/payments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewPayment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewPayment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PaymentIn>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PaymentIn)ApiClient.Deserialize(localVarResponse, typeof(PaymentIn)));
            
        }
        /// <summary>
        /// Get new shipment for specified customer order Return new shipment document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Shipment</returns>
        public Shipment OrderModuleGetNewShipment(string id)
        {
             ApiResponse<Shipment> localVarResponse = OrderModuleGetNewShipmentWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get new shipment for specified customer order Return new shipment document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>ApiResponse of Shipment</returns>
        public ApiResponse<Shipment> OrderModuleGetNewShipmentWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetNewShipment");

            var localVarPath = "/api/order/customerOrders/{id}/shipments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewShipment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewShipment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Shipment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Shipment)ApiClient.Deserialize(localVarResponse, typeof(Shipment)));
            
        }

        /// <summary>
        /// Get new shipment for specified customer order Return new shipment document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of Shipment</returns>
        public async System.Threading.Tasks.Task<Shipment> OrderModuleGetNewShipmentAsync(string id)
        {
             ApiResponse<Shipment> localVarResponse = await OrderModuleGetNewShipmentAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get new shipment for specified customer order Return new shipment document with populates all required properties.
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">customer order id</param>
        /// <returns>Task of ApiResponse (Shipment)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Shipment>> OrderModuleGetNewShipmentAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceOrdersApi->OrderModuleGetNewShipment");

            var localVarPath = "/api/order/customerOrders/{id}/shipments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewShipment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleGetNewShipment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Shipment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Shipment)ApiClient.Deserialize(localVarResponse, typeof(Shipment)));
            
        }
        /// <summary>
        /// Register customer order payment in external payment system Used in storefront checkout or manual order payment registration
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>ProcessPaymentResult</returns>
        public ProcessPaymentResult OrderModuleProcessOrderPayments(string orderId, string paymentId, BankCardInfo bankCardInfo = null)
        {
             ApiResponse<ProcessPaymentResult> localVarResponse = OrderModuleProcessOrderPaymentsWithHttpInfo(orderId, paymentId, bankCardInfo);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Register customer order payment in external payment system Used in storefront checkout or manual order payment registration
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>ApiResponse of ProcessPaymentResult</returns>
        public ApiResponse<ProcessPaymentResult> OrderModuleProcessOrderPaymentsWithHttpInfo(string orderId, string paymentId, BankCardInfo bankCardInfo = null)
        {
            // verify the required parameter 'orderId' is set
            if (orderId == null)
                throw new ApiException(400, "Missing required parameter 'orderId' when calling VirtoCommerceOrdersApi->OrderModuleProcessOrderPayments");
            // verify the required parameter 'paymentId' is set
            if (paymentId == null)
                throw new ApiException(400, "Missing required parameter 'paymentId' when calling VirtoCommerceOrdersApi->OrderModuleProcessOrderPayments");

            var localVarPath = "/api/order/customerOrders/{orderId}/processPayment/{paymentId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (orderId != null) localVarPathParams.Add("orderId", ApiClient.ParameterToString(orderId)); // path parameter
            if (paymentId != null) localVarPathParams.Add("paymentId", ApiClient.ParameterToString(paymentId)); // path parameter
            if (bankCardInfo != null && bankCardInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(bankCardInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = bankCardInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleProcessOrderPayments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleProcessOrderPayments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ProcessPaymentResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ProcessPaymentResult)ApiClient.Deserialize(localVarResponse, typeof(ProcessPaymentResult)));
            
        }

        /// <summary>
        /// Register customer order payment in external payment system Used in storefront checkout or manual order payment registration
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>Task of ProcessPaymentResult</returns>
        public async System.Threading.Tasks.Task<ProcessPaymentResult> OrderModuleProcessOrderPaymentsAsync(string orderId, string paymentId, BankCardInfo bankCardInfo = null)
        {
             ApiResponse<ProcessPaymentResult> localVarResponse = await OrderModuleProcessOrderPaymentsAsyncWithHttpInfo(orderId, paymentId, bankCardInfo);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Register customer order payment in external payment system Used in storefront checkout or manual order payment registration
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information (optional)</param>
        /// <returns>Task of ApiResponse (ProcessPaymentResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<ProcessPaymentResult>> OrderModuleProcessOrderPaymentsAsyncWithHttpInfo(string orderId, string paymentId, BankCardInfo bankCardInfo = null)
        {
            // verify the required parameter 'orderId' is set
            if (orderId == null)
                throw new ApiException(400, "Missing required parameter 'orderId' when calling VirtoCommerceOrdersApi->OrderModuleProcessOrderPayments");
            // verify the required parameter 'paymentId' is set
            if (paymentId == null)
                throw new ApiException(400, "Missing required parameter 'paymentId' when calling VirtoCommerceOrdersApi->OrderModuleProcessOrderPayments");

            var localVarPath = "/api/order/customerOrders/{orderId}/processPayment/{paymentId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (orderId != null) localVarPathParams.Add("orderId", ApiClient.ParameterToString(orderId)); // path parameter
            if (paymentId != null) localVarPathParams.Add("paymentId", ApiClient.ParameterToString(paymentId)); // path parameter
            if (bankCardInfo != null && bankCardInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(bankCardInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = bankCardInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleProcessOrderPayments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleProcessOrderPayments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ProcessPaymentResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ProcessPaymentResult)ApiClient.Deserialize(localVarResponse, typeof(ProcessPaymentResult)));
            
        }
        /// <summary>
        /// Search customer orders by given criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>SearchResult</returns>
        public SearchResult OrderModuleSearch(SearchCriteria criteria)
        {
             ApiResponse<SearchResult> localVarResponse = OrderModuleSearchWithHttpInfo(criteria);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Search customer orders by given criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>ApiResponse of SearchResult</returns>
        public ApiResponse<SearchResult> OrderModuleSearchWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceOrdersApi->OrderModuleSearch");

            var localVarPath = "/api/order/customerOrders/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<SearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (SearchResult)ApiClient.Deserialize(localVarResponse, typeof(SearchResult)));
            
        }

        /// <summary>
        /// Search customer orders by given criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>Task of SearchResult</returns>
        public async System.Threading.Tasks.Task<SearchResult> OrderModuleSearchAsync(SearchCriteria criteria)
        {
             ApiResponse<SearchResult> localVarResponse = await OrderModuleSearchAsyncWithHttpInfo(criteria);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Search customer orders by given criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">criteria</param>
        /// <returns>Task of ApiResponse (SearchResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<SearchResult>> OrderModuleSearchAsyncWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceOrdersApi->OrderModuleSearch");

            var localVarPath = "/api/order/customerOrders/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<SearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (SearchResult)ApiClient.Deserialize(localVarResponse, typeof(SearchResult)));
            
        }
        /// <summary>
        /// Update a existing customer order 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns></returns>
        public void OrderModuleUpdate(CustomerOrder customerOrder)
        {
             OrderModuleUpdateWithHttpInfo(customerOrder);
        }

        /// <summary>
        /// Update a existing customer order 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> OrderModuleUpdateWithHttpInfo(CustomerOrder customerOrder)
        {
            // verify the required parameter 'customerOrder' is set
            if (customerOrder == null)
                throw new ApiException(400, "Missing required parameter 'customerOrder' when calling VirtoCommerceOrdersApi->OrderModuleUpdate");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (customerOrder.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(customerOrder); // http body (model) parameter
            }
            else
            {
                localVarPostBody = customerOrder; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update a existing customer order 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task OrderModuleUpdateAsync(CustomerOrder customerOrder)
        {
             await OrderModuleUpdateAsyncWithHttpInfo(customerOrder);

        }

        /// <summary>
        /// Update a existing customer order 
        /// </summary>
        /// <exception cref="VirtoCommerce.OrderModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="customerOrder">customer order</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> OrderModuleUpdateAsyncWithHttpInfo(CustomerOrder customerOrder)
        {
            // verify the required parameter 'customerOrder' is set
            if (customerOrder == null)
                throw new ApiException(400, "Missing required parameter 'customerOrder' when calling VirtoCommerceOrdersApi->OrderModuleUpdate");

            var localVarPath = "/api/order/customerOrders";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (customerOrder.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(customerOrder); // http body (model) parameter
            }
            else
            {
                localVarPostBody = customerOrder; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling OrderModuleUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
    }
}
