using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CartModule.Core.Services;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Notifications;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;
using VirtoCommerce.OrdersModule.Data.Caching;
using VirtoCommerce.OrdersModule.Data.Extensions;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.OrdersModule.Web.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Data;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Json;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using CustomerOrderSearchResult = VirtoCommerce.OrdersModule.Core.Model.Search.CustomerOrderSearchResult;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/customerOrders")]
    [Authorize]
    public class OrderModuleController : Controller
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICrudService<CustomerOrder> _customerOrderServiceCrud;
        private readonly ISearchService<CustomerOrderSearchCriteria, CustomerOrderSearchResult, CustomerOrder> _searchService;
        private readonly IUniqueNumberGenerator _uniqueNumberGenerator;
        private readonly IStoreService _storeService;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly ICustomerOrderStatisticService _customerOrderStatisticService;
        private readonly ICustomerOrderBuilder _customerOrderBuilder;
        private readonly IShoppingCartService _cartService;
        private readonly ICustomerOrderTotalsCalculator _totalsCalculator;
        private readonly INotificationSearchService _notificationSearchService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotificationTemplateRenderer _notificationTemplateRenderer;
        private readonly IChangeLogSearchService _changeLogSearchService;
        private readonly IConverter _converter;
        private readonly IIndexedCustomerOrderSearchService _indexedSearchService;
        private readonly IConfiguration _configuration;
        private readonly IValidator<CustomerOrder> _customerOrderValidator;
        private readonly ISettingsManager _settingsManager;
        private readonly HtmlToPdfOptions _htmlToPdfOptions;
        private readonly OutputJsonSerializerSettings _outputJsonSerializerSettings;

        public OrderModuleController(
              ICustomerOrderService customerOrderService
            , ICustomerOrderSearchService searchService
            , IStoreService storeService
            , IUniqueNumberGenerator numberGenerator
            , IPlatformMemoryCache platformMemoryCache
            , ICustomerOrderStatisticService customerOrderStatisticService
            , ICustomerOrderBuilder customerOrderBuilder
            , IShoppingCartService cartService
            , IChangeLogSearchService changeLogSearchService
            , INotificationTemplateRenderer notificationTemplateRenderer
            , INotificationSearchService notificationSearchService
            , ICustomerOrderTotalsCalculator totalsCalculator
            , IAuthorizationService authorizationService
            , IConverter converter
            , IIndexedCustomerOrderSearchService indexedSearchService
            , IConfiguration configuration
            , IOptions<HtmlToPdfOptions> htmlToPdfOptions
            , IOptions<OutputJsonSerializerSettings> outputJsonSerializerSettings
            , IValidator<CustomerOrder> customerOrderValidator
            , ISettingsManager settingsManager)
        {
            _customerOrderService = customerOrderService;
            _customerOrderServiceCrud = (ICrudService<CustomerOrder>)customerOrderService;
            _searchService = (ISearchService<CustomerOrderSearchCriteria, CustomerOrderSearchResult, CustomerOrder>)searchService;
            _uniqueNumberGenerator = numberGenerator;
            _storeService = storeService;
            _platformMemoryCache = platformMemoryCache;
            _customerOrderStatisticService = customerOrderStatisticService;
            _customerOrderBuilder = customerOrderBuilder;
            _cartService = cartService;
            _changeLogSearchService = changeLogSearchService;
            _notificationTemplateRenderer = notificationTemplateRenderer;
            _notificationSearchService = notificationSearchService;
            _totalsCalculator = totalsCalculator;
            _authorizationService = authorizationService;
            _converter = converter;
            _indexedSearchService = indexedSearchService;
            _configuration = configuration;
            _customerOrderValidator = customerOrderValidator;
            _settingsManager = settingsManager;
            _htmlToPdfOptions = htmlToPdfOptions.Value;
            _outputJsonSerializerSettings = outputJsonSerializerSettings.Value;
        }

        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<CustomerOrderSearchResult>> SearchCustomerOrder([FromBody] CustomerOrderSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _searchService.SearchAsync(criteria);
            //It is a important to return serialized data by such way. Instead you have a slow response time for large outputs 
            //https://github.com/dotnet/aspnetcore/issues/19646
            return Content(JsonConvert.SerializeObject(result, _outputJsonSerializerSettings), "application/json");
        }

        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>Return a single customer order with all nested documents or null if order was not found</remarks>
        /// <param name="number">customer order number</param>
        /// <param name="respGroup"></param>
        [HttpGet]
        [Route("number/{number}")]
        public async Task<ActionResult<CustomerOrder>> GetByNumber(string number, [SwaggerOptional][FromQuery] string respGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Number = number;
            searchCriteria.ResponseGroup = respGroup;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _searchService.SearchAsync(searchCriteria);

            var retVal = result.Results.FirstOrDefault();
            return Ok(retVal);
        }

        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>Return a single customer order with all nested documents or null if order was not found</remarks>
        /// <param name="id">customer order id</param>
        /// <param name="respGroup"></param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CustomerOrder>> GetById(string id, [SwaggerOptional][FromQuery] string respGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Ids = new[] { id };
            searchCriteria.ResponseGroup = respGroup;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _searchService.SearchAsync(searchCriteria);

            return Ok(result.Results.FirstOrDefault());
        }

        /// <summary>
        /// Calculate order totals after changes
        /// </summary>
        /// <remarks>Return order with recalculated totals</remarks>
        /// <param name="customerOrder">Customer order</param>
        [HttpPut]
        [Route("recalculate")]
        public async Task<ActionResult<CustomerOrder>> CalculateTotals([FromBody] CustomerOrder customerOrder)
        {
            var validationResult = await ValidateAsync(customerOrder);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    Errors = validationResult.Errors
                });
            }
            _totalsCalculator.CalculateTotals(customerOrder);
            customerOrder.FillAllChildOperations();

            return Ok(customerOrder);
        }

        /// <summary>
        /// Register customer order payment in external payment system (without bankCardInfo).
        /// It's a workaround method, allowing to accept requests without bankCardInfo.
        /// </summary>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        [HttpPost]
        [Route("{orderId}/processPayment/{paymentId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<ActionResult<ProcessPaymentRequestResult>> ProcessOrderPaymentsWithoutBankCardInfo([FromRoute] string orderId, [FromRoute] string paymentId)
        {
            return ProcessOrderPayments(orderId, paymentId, null);
        }

        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>Used in storefront checkout or manual order payment registration</remarks>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information</param>
        [HttpPost]
        [Route("{orderId}/processPayment/{paymentId}")]
        [Consumes("application/json", new[] { "application/json-patch+json" })] // It's a trick that allows ASP.NET infrastructure to select this action with body and ProcessOrderPaymentsWithoutBankCardInfo if no body
        public async Task<ActionResult<ProcessPaymentRequestResult>> ProcessOrderPayments([FromRoute] string orderId, [FromRoute] string paymentId, [FromBody] BankCardInfo bankCardInfo)
        {
            var customerOrder = await _customerOrderServiceCrud.GetByIdAsync(orderId, CustomerOrderResponseGroup.Full.ToString());

            if (customerOrder == null)
            {
                var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
                searchCriteria.Number = orderId;
                searchCriteria.ResponseGroup = CustomerOrderResponseGroup.Full.ToString();

                var orders = await _searchService.SearchAsync(searchCriteria);
                customerOrder = orders.Results.FirstOrDefault();
            }

            if (customerOrder == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {orderId}");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var inPayment = customerOrder.InPayments.FirstOrDefault(x => x.Id == paymentId);
            if (inPayment == null)
            {
                throw new InvalidOperationException($"Cannot find payment with ID {paymentId}");
            }
            if (inPayment.PaymentMethod == null)
            {
                throw new InvalidOperationException($"Cannot find payment method with code {inPayment.GatewayCode}");
            }

            var store = await _storeService.GetByIdAsync(customerOrder.StoreId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                throw new InvalidOperationException($"Cannot find store with ID {customerOrder.StoreId}");
            }

            var request = new ProcessPaymentRequest
            {
                OrderId = customerOrder.Id,
                Order = customerOrder,
                PaymentId = inPayment.Id,
                Payment = inPayment,
                StoreId = customerOrder.StoreId,
                Store = store,
                BankCardInfo = bankCardInfo
            };
            var result = inPayment.PaymentMethod.ProcessPayment(request);
            if (result.OuterId != null)
            {
                inPayment.OuterId = result.OuterId;
            }

            // Exclusive status set for DefaultManualPaymentMethod, otherwise the order be stuck in the "New" state.
            // Current payment flow suggests payment processing by payment method code,
            // but, unfortunately, there is no way to do set status directly in DefaultManualPaymentMethod, because it will produce cyclical references between order and payment modules.
            // One day, we should change the flow to commonly divide payment and order processing, but now it isn't.
            if (inPayment.PaymentMethod is DefaultManualPaymentMethod)
            {
                customerOrder.Status = result.NewPaymentStatus.ToString();
            }

            var validationResult = await ValidateAsync(customerOrder);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    Errors = validationResult.Errors
                });
            }

            await _customerOrderService.SaveChangesAsync(new[] { customerOrder });

            return Ok(result);
        }

        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <param name="cartId">shopping cart id</param>
        [HttpPost]
        [Route("{cartId}")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<CustomerOrder>> CreateOrderFromCart(string cartId)
        {
            CustomerOrder retVal = null;

            using (await AsyncLock.GetLockByKey(cartId).LockAsync())
            {
                var cart = await _cartService.GetByIdAsync(cartId);
                retVal = await _customerOrderBuilder.PlaceCustomerOrderFromCartAsync(cart);
            }

            return Ok(retVal);
        }

        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <param name="customerOrder">customer order</param>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<CustomerOrder>> CreateOrder([FromBody] CustomerOrder customerOrder)
        {
            var validationResult = await ValidateAsync(customerOrder);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    Errors = validationResult.Errors
                });
            }

            await _customerOrderService.SaveChangesAsync(new[] { customerOrder });
            return Ok(customerOrder);
        }

        /// <summary>
        ///  Update a existing customer order 
        /// </summary>
        /// <param name="customerOrder">customer order</param>
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateOrder([FromBody] CustomerOrder customerOrder)
        {
            var order = await _customerOrderServiceCrud.GetByIdAsync(customerOrder.Id);
            if (order == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var validationResult = await ValidateAsync(customerOrder);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    Errors = validationResult.Errors
                });
            }

            await _customerOrderService.SaveChangesAsync(new[] { customerOrder });
            return NoContent();
        }

        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>Return new shipment document with populates all required properties.</remarks>
        /// <param name="id">customer order id </param>
        [HttpGet]
        [Route("{id}/shipments/new")]
        public async Task<ActionResult<Shipment>> GetNewShipment(string id)
        {
            var order = await _customerOrderServiceCrud.GetByIdAsync(id, CustomerOrderResponseGroup.Full.ToString());
            if (order != null)
            {
                var retVal = AbstractTypeFactory<Shipment>.TryCreateInstance();
                retVal.Id = Guid.NewGuid().ToString();
                retVal.Currency = order.Currency;
                retVal.Status = "New";

                var store = await _storeService.GetByIdAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
                var numberTemplate = store.Settings.GetSettingValue(
                    ModuleConstants.Settings.General.OrderShipmentNewNumberTemplate.Name,
                    ModuleConstants.Settings.General.OrderShipmentNewNumberTemplate.DefaultValue);
                retVal.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate.ToString());

                return Ok(retVal);

                ////Detect not whole shipped items
                ////TODO: LineItem partial shipping
                //var shippedLineItemIds = order.Shipments.SelectMany(x => x.Items).Select(x => x.LineItemId);

                ////TODO Add check for digital products (don't add to shipment)
                //retVal.Items = order.Items.Where(x => !shippedLineItemIds.Contains(x.Id))
                //              .Select(x => new coreModel.ShipmentItem(x)).ToList();
                //return Ok(retVal.ToWebModel());
            }

            return NotFound();
        }

        /// <summary>
        /// Get new payment for specified customer order
        /// </summary>
        /// <remarks>Return new payment  document with populates all required properties.</remarks>
        /// <param name="id">customer order id </param>
        [HttpGet]
        [Route("{id}/payments/new")]
        public async Task<ActionResult<PaymentIn>> GetNewPayment(string id)
        {
            var order = await _customerOrderServiceCrud.GetByIdAsync(id, CustomerOrderResponseGroup.Full.ToString());
            if (order != null)
            {
                var retVal = AbstractTypeFactory<PaymentIn>.TryCreateInstance();
                retVal.Id = Guid.NewGuid().ToString();
                retVal.Currency = order.Currency;
                retVal.CustomerId = order.CustomerId;
                retVal.Status = retVal.PaymentStatus.ToString();

                var store = await _storeService.GetByIdAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
                var numberTemplate = store.Settings.GetSettingValue(
                    ModuleConstants.Settings.General.OrderPaymentInNewNumberTemplate.Name,
                    ModuleConstants.Settings.General.OrderPaymentInNewNumberTemplate.DefaultValue);
                retVal.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate.ToString());
                return Ok(retVal);
            }

            return NotFound();
        }

        /// <summary>
        ///  Delete a whole customer orders
        /// </summary>
        /// <param name="ids">customer order ids for delete</param>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteOrdersByIds([FromQuery] string[] ids)
        {
            var unauthorizedRequest = false;

            foreach (var id in ids)
            {
                var customerOrder = await _customerOrderServiceCrud.GetByIdAsync(id);
                if (customerOrder == null)
                {
                    continue;
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
                if (authorizationResult.Succeeded)
                {
                    await _customerOrderService.DeleteAsync(new[] { id });
                }
                else
                {
                    unauthorizedRequest = true;
                }
            }

            return unauthorizedRequest ? Forbid() : NoContent();
        }

        /// <summary>
        ///  Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <param name="start">start interval date</param>
        /// <param name="end">end interval date</param>
        [HttpGet]
        [Route("~/api/order/dashboardStatistics")]
        [Authorize(ModuleConstants.Security.Permissions.ViewDashboardStatistic)]
        public async Task<ActionResult<DashboardStatisticsResult>> GetDashboardStatisticsAsync([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            start ??= DateTime.UtcNow.AddYears(-1);
            end ??= DateTime.UtcNow;

            // Hack: to compinsate for incorrect Local dates to UTC
            end = end.Value.AddDays(2);

            var cacheKey = CacheKey.With(GetType(), string.Join(":", "Statistic", start.Value.ToString("yyyy-MM-dd"), end.Value.ToString("yyyy-MM-dd")));
            var retVal = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(OrderSearchCacheRegion.CreateChangeToken());

                var result = await _customerOrderStatisticService.CollectStatisticsAsync(start.Value, end.Value);
                return result;
            });

            return Ok(retVal);
        }

        /// <summary>
        /// Payment callback operation used by external payment services to inform post process payment in our system
        /// </summary>
        /// <param name="callback">payment callback parameters</param>
        [HttpPost]
        [Route("~/api/paymentcallback")]
        public async Task<ActionResult<PostProcessPaymentRequestResult>> PostProcessPayment([FromBody] PaymentCallbackParameters callback)
        {
            var parameters = new NameValueCollection();
            foreach (var param in callback?.Parameters ?? Array.Empty<KeyValuePair>())
            {
                parameters.Add(param.Key, param.Value);
            }
            var orderId = parameters.Get("orderid");
            if (string.IsNullOrEmpty(orderId))
            {
                throw new InvalidOperationException("the 'orderid' parameter must be passed");
            }

            //some payment method require customer number to be passed and returned. First search customer order by number
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Number = orderId;
            searchCriteria.ResponseGroup = CustomerOrderResponseGroup.Full.ToString();
            //if order not found by order number search by order id
            var orders = await _searchService.SearchAsync(searchCriteria);
            var customerOrder = orders.Results.FirstOrDefault() ?? await _customerOrderServiceCrud.GetByIdAsync(orderId, CustomerOrderResponseGroup.Full.ToString());

            if (customerOrder == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {orderId}");
            }

            var store = await _storeService.GetByIdAsync(customerOrder.StoreId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                throw new InvalidOperationException($"Cannot find store with ID {customerOrder.StoreId}");
            }

            var paymentMethodCode = parameters.Get("code");

            //Need to use concrete  payment method if it code passed otherwise use all order payment methods
            foreach (var inPayment in customerOrder.InPayments.Where(x => x.PaymentMethod != null && (string.IsNullOrEmpty(paymentMethodCode) || x.GatewayCode.EqualsInvariant(paymentMethodCode))))
            {
                //Each payment method must check that these parameters are addressed to it
                var result = inPayment.PaymentMethod.ValidatePostProcessRequest(parameters);
                if (result.IsSuccess)
                {

                    var request = new PostProcessPaymentRequest
                    {
                        OrderId = customerOrder.Id,
                        Order = customerOrder,
                        PaymentId = inPayment.Id,
                        Payment = inPayment,
                        StoreId = customerOrder.StoreId,
                        Store = store,
                        OuterId = result.OuterId,
                        Parameters = parameters
                    };
                    var retVal = inPayment.PaymentMethod.PostProcessPayment(request);
                    if (retVal != null)
                    {
                        var validationResult = await ValidateAsync(customerOrder);
                        if (!validationResult.IsValid)
                        {
                            return BadRequest(new
                            {
                                Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                                Errors = validationResult.Errors
                            });
                        }
                        await _customerOrderService.SaveChangesAsync(new[] { customerOrder });

                        // order Number is required
                        retVal.OrderId = customerOrder.Number;
                    }
                    return Ok(retVal);
                }
            }
            return Ok(new PostProcessPaymentRequestResult { ErrorMessage = "Payment method not found" });
        }

        [HttpGet]
        [Route("invoice/{orderNumber}")]
        [SwaggerFileResponse]
        public async Task<ActionResult> GetInvoicePdf(string orderNumber)
        {
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Number = orderNumber;
            searchCriteria.Take = 1;
            //ToDo
            //searchCriteria.ResponseGroup = OrderReadPricesPermission.ApplyResponseGroupFiltering(_securityService.GetUserPermissions(User.Identity.Name), null);

            var orders = await _searchService.SearchAsync(searchCriteria);
            var order = orders.Results.FirstOrDefault();

            if (order == null)
            {
                throw new InvalidOperationException($"Cannot find order with number {orderNumber}");
            }

            var notification = await _notificationSearchService.GetNotificationAsync<InvoiceEmailNotification>(new TenantIdentity(order.StoreId, nameof(Store)));
            notification.CustomerOrder = order;
            notification.LanguageCode = order.LanguageCode;
            var message = AbstractTypeFactory<EmailNotificationMessage>.TryCreateInstance($"{notification.Kind}Message");
            await notification.ToMessageAsync(message, _notificationTemplateRenderer);

            if (message.Body.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"Document could not be rendered because InvoiceEmailNotification template is empty.");
            }

            byte[] result = GeneratePdf(message.Body);

            return new FileContentResult(result, "application/pdf");
        }

        [HttpGet]
        [Route("{id}/changes")]
        public async Task<ActionResult<OperationLog[]>> GetOrderChanges(string id)
        {
            var result = Array.Empty<OperationLog>();
            var order = await _customerOrderServiceCrud.GetByIdAsync(id);
            if (order != null)
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                //Load general change log for order
                var allHasChangesObjects = order.GetFlatObjectsListWithInterface<IHasChangesHistory>()
                                          .Distinct().ToArray();

                var criteria = new ChangeLogSearchCriteria
                {
                    ObjectIds = allHasChangesObjects.Select(x => x.Id).Distinct().ToArray(),
                    ObjectTypes = allHasChangesObjects.Select(x => x.GetType().Name).Distinct().ToArray()
                };
                result = (await _changeLogSearchService.SearchAsync(criteria)).Results.ToArray();

            }
            return Ok(result);
        }


        [HttpPost]
        [Route("searchChanges")]
        public async Task<ActionResult<ChangeLogSearchResult>> SearchOrderChanges([FromBody] CustomerOrderHistorySearchCriteria historySearchCriteria)
        {
            if (historySearchCriteria.OrderId == null)
            {
                throw new InvalidOperationException($"Order ID can not be null");
            }

            var order = await _customerOrderServiceCrud.GetByIdAsync(historySearchCriteria.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {historySearchCriteria.OrderId}");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            //Load general change log for order
            var allHasChangesObjects = order.GetFlatObjectsListWithInterface<IHasChangesHistory>()
                .Distinct().ToArray();

            var criteria = new ChangeLogSearchCriteria
            {
                ObjectIds = allHasChangesObjects.Select(x => x.Id).Distinct().ToArray(),
                ObjectTypes = allHasChangesObjects.Select(x => x.GetType().Name).Distinct().ToArray(),
                Skip = historySearchCriteria.Skip,
                Take = historySearchCriteria.Take,
                Sort = historySearchCriteria.Sort
            };

            var result = await _changeLogSearchService.SearchAsync(criteria);
            return Ok(result);
        }

        [HttpGet]
        [Route("indexed/searchEnabled")]
        public ActionResult GetOrderFullTextSearchEnabled()
        {
            var result = _configuration.IsOrderFullTextSearchEnabled();
            return Ok(new { Result = result });
        }

        [HttpPost]
        [Route("indexed/search")]
        public async Task<ActionResult<CustomerOrderSearchResult>> SearchCustomerOrderIndexed([FromBody] CustomerOrderIndexedSearchCriteria criteria)
        {
            var result = await _indexedSearchService.SearchCustomerOrdersAsync(criteria);
            return Content(JsonConvert.SerializeObject(result, _outputJsonSerializerSettings), "application/json");
        }

        private byte[] GeneratePdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    PaperSize = EnumUtility.SafeParse(_htmlToPdfOptions.PaperSize, PaperKind.A4),
                    ViewportSize = _htmlToPdfOptions.ViewportSize,
                    DPI = _htmlToPdfOptions.DPI
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = _htmlToPdfOptions.DefaultEncoding, MinimumFontSize = _htmlToPdfOptions.MinimumFontSize },
                    }
                }
            };
            var result = _converter.Convert(doc);

            return result;
        }

        private Task<ValidationResult> ValidateAsync(CustomerOrder customerOrder)
        {
            if (_settingsManager.GetValue(ModuleConstants.Settings.General.CustomerOrderValidation.Name, (bool)ModuleConstants.Settings.General.CustomerOrderValidation.DefaultValue))
            {
                return _customerOrderValidator.ValidateAsync(customerOrder);
            }

            return Task.FromResult(new ValidationResult());
        }
    }
}
