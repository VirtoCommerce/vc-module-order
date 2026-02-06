using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Data;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Json;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using CustomerOrderSearchResult = VirtoCommerce.OrdersModule.Core.Model.Search.CustomerOrderSearchResult;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/customerOrders")]
    [Authorize]
    public class OrderModuleController(
        ICustomerOrderService customerOrderService,
        ICustomerOrderSearchService searchService,
        IStoreService storeService,
        ITenantUniqueNumberGenerator numberGenerator,
        IPlatformMemoryCache platformMemoryCache,
        ICustomerOrderStatisticService customerOrderStatisticService,
        ICustomerOrderBuilder customerOrderBuilder,
        IShoppingCartService cartService,
        IChangeLogSearchService changeLogSearchService,
        INotificationTemplateRenderer notificationTemplateRenderer,
        INotificationSearchService notificationSearchService,
        ICustomerOrderTotalsCalculator totalsCalculator,
        IAuthorizationService authorizationService,
        IConverter converter,
        IIndexedCustomerOrderSearchService indexedSearchService,
        IConfiguration configuration,
        IOptions<HtmlToPdfOptions> htmlToPdfOptions,
        IOptions<OutputJsonSerializerSettings> outputJsonSerializerSettings,
        IValidator<CustomerOrder> customerOrderValidator,
        ISettingsManager settingsManager,
        IPaymentRequestConverter paymentRequestConverter,
        ICustomerOrderPaymentService customerOrderPaymentService)
        : Controller
    {
        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<CustomerOrderSearchResult>> SearchCustomerOrder([FromBody] CustomerOrderSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await searchService.SearchAsync(criteria);
            //It is a important to return serialized data by such way. Instead you have a slow response time for large outputs 
            //https://github.com/dotnet/aspnetcore/issues/19646
            return Content(JsonConvert.SerializeObject(result, outputJsonSerializerSettings.Value), "application/json");
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
            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await searchService.SearchAsync(searchCriteria);

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
            searchCriteria.Ids = [id];
            searchCriteria.ResponseGroup = respGroup;
            searchCriteria.WithPrototypes = true;

            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await searchService.SearchAsync(searchCriteria);

            return Ok(result.Results.FirstOrDefault());
        }

        /// <summary>
        /// Find customer order by outer id
        /// </summary>
        /// <remarks>Return a single customer order with all nested documents or null if order was not found</remarks>
        /// <param name="outerId">customer order outer id</param>
        /// <param name="responseGroup"></param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<CustomerOrder>> GetByOuterId(string outerId, [SwaggerOptional][FromQuery] string responseGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.OuterIds = [outerId];
            searchCriteria.ResponseGroup = responseGroup;
            searchCriteria.WithPrototypes = true;

            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await searchService.SearchAsync(searchCriteria);

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
                    validationResult.Errors
                });
            }
            totalsCalculator.CalculateTotals(customerOrder);
            customerOrder.FillAllChildOperations();

            return Ok(customerOrder);
        }

        /// <summary>
        /// Register customer order payment in external payment system (without bankCardInfo).
        /// It's a workaround method, allowing to accept requests without bankCardInfo.
        /// </summary>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="cancellationToken">cancellationToken</param>
        [HttpPost]
        [Route("{orderId}/processPayment/{paymentId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<ActionResult<ProcessPaymentRequestResult>> ProcessOrderPaymentsWithoutBankCardInfo([FromRoute] string orderId, [FromRoute] string paymentId, CancellationToken cancellationToken)
        {
            return ProcessOrderPayments(orderId, paymentId, null, cancellationToken);
        }

        /// <summary>
        /// Register customer order payment in external payment system
        /// </summary>
        /// <remarks>Used in storefront checkout or manual order payment registration</remarks>
        /// <param name="orderId">customer order id</param>
        /// <param name="paymentId">payment id</param>
        /// <param name="bankCardInfo">banking card information</param>
        /// <param name="cancellationToken">cancellationToken</param>
        [HttpPost]
        [Route("{orderId}/processPayment/{paymentId}")]
        [Consumes("application/json", "application/json-patch+json")] // It's a trick that allows ASP.NET infrastructure to select this action with body and ProcessOrderPaymentsWithoutBankCardInfo if no body
        public async Task<ActionResult<ProcessPaymentRequestResult>> ProcessOrderPayments([FromRoute] string orderId, [FromRoute] string paymentId, [FromBody] BankCardInfo bankCardInfo, CancellationToken cancellationToken)
        {
            var customerOrder = await customerOrderService.GetByIdAsync(orderId, CustomerOrderResponseGroup.Full.ToString());

            if (customerOrder == null)
            {
                var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
                searchCriteria.Number = orderId;
                searchCriteria.ResponseGroup = CustomerOrderResponseGroup.Full.ToString();

                var orders = await searchService.SearchAsync(searchCriteria);
                customerOrder = orders.Results.FirstOrDefault();
            }

            if (customerOrder == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {orderId}");
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
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

            var store = await storeService.GetByIdAsync(customerOrder.StoreId, StoreResponseGroup.StoreInfo.ToString());
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
            var result = await inPayment.PaymentMethod.ProcessPaymentAsync(request, cancellationToken);
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

            await customerOrderService.SaveChangesAsync([customerOrder]);

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
            CustomerOrder retVal;

            using (await AsyncLock.GetLockByKey(cartId).LockAsync())
            {
                var cart = await cartService.GetByIdAsync(cartId);
                retVal = await customerOrderBuilder.PlaceCustomerOrderFromCartAsync(cart);
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
                    validationResult.Errors
                });
            }

            await customerOrderService.SaveChangesAsync([customerOrder]);
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
            var order = await customerOrderService.GetByIdAsync(customerOrder.Id);
            if (order == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
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
                    validationResult.Errors
                });
            }

            if (!User.HasGlobalPermission(ModuleConstants.Security.Permissions.ReadPrices))
            {
                // Restore prices from order if user has no ReadPrices permission and receive the order without prices
                customerOrder.RestoreDetails(order);
            }

            try
            {
                await customerOrderService.SaveChangesAsync(new[] { customerOrder });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

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
            var order = await customerOrderService.GetNoCloneAsync(id, CustomerOrderResponseGroup.Full.ToString());
            if (order == null)
            {
                return NotFound();
            }

            var store = await storeService.GetNoCloneAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                return BadRequest(GetNoStoreErrorMessage(order));
            }

            var retVal = AbstractTypeFactory<Shipment>.TryCreateInstance();
            retVal.Id = Guid.NewGuid().ToString();
            retVal.Currency = order.Currency;
            retVal.Status = "New";

            var numberTemplate = store.Settings.GetValue<string>(ModuleConstants.Settings.General.OrderShipmentNewNumberTemplate);
            retVal.Number = numberGenerator.GenerateNumber(store.Id, numberTemplate);

            return Ok(retVal);
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
            var order = await customerOrderService.GetNoCloneAsync(id, CustomerOrderResponseGroup.Full.ToString());
            if (order == null)
            {
                return NotFound();
            }

            var store = await storeService.GetNoCloneAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                return BadRequest(GetNoStoreErrorMessage(order));
            }

            var retVal = AbstractTypeFactory<PaymentIn>.TryCreateInstance();
            retVal.Id = Guid.NewGuid().ToString();
            retVal.Currency = order.Currency;
            retVal.CustomerId = order.CustomerId;
            retVal.Status = retVal.PaymentStatus.ToString();

            var numberTemplate = store.Settings.GetValue<string>(ModuleConstants.Settings.General.OrderPaymentInNewNumberTemplate);
            retVal.Number = numberGenerator.GenerateNumber(store.Id, numberTemplate);
            return Ok(retVal);
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
                var customerOrder = await customerOrderService.GetByIdAsync(id);
                if (customerOrder == null)
                {
                    continue;
                }

                var authorizationResult = await authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
                if (authorizationResult.Succeeded)
                {
                    await customerOrderService.DeleteAsync(new[] { id });
                }
                else
                {
                    unauthorizedRequest = true;
                }
            }

            return unauthorizedRequest ? Forbid() : NoContent();
        }

        /// <summary>
        /// Get dashboard statistics settings
        /// </summary>
        [HttpGet]
        [Route("~/api/order/dashboardStatistics/settings")]
        [Authorize(ModuleConstants.Security.Permissions.ViewDashboardStatistics)]
        public async Task<ActionResult<object>> GetDashboardStatisticsSettingsAsync()
        {
            var enabled = await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.DashboardStatisticsEnabled);
            var rangeMonths = await settingsManager.GetValueAsync<int>(ModuleConstants.Settings.General.DashboardStatisticsRangeMonths);

            return Ok(new
            {
                Enabled = enabled,
                RangeMonths = rangeMonths
            });
        }

        /// <summary>
        ///  Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <param name="start">start interval date</param>
        /// <param name="end">end interval date</param>
        [HttpGet]
        [Route("~/api/order/dashboardStatistics")]
        [Authorize(ModuleConstants.Security.Permissions.ViewDashboardStatistics)]
        public async Task<ActionResult<DashboardStatisticsResult>> GetDashboardStatisticsAsync([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            var dashboardEnabled = await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.DashboardStatisticsEnabled);
            if (!dashboardEnabled)
            {
                return Ok(new DashboardStatisticsResult());
            }

            if (start == null)
            {
                var rangeMonths = await settingsManager.GetValueAsync<int>(ModuleConstants.Settings.General.DashboardStatisticsRangeMonths);
                start = DateTime.UtcNow.AddMonths(-rangeMonths);
            }

            end ??= DateTime.UtcNow;

            // Hack: to compensate for incorrect Local dates to UTC
            end = end.Value.AddDays(2);

            var cacheKey = CacheKey.With(GetType(), string.Join(":", "Statistic", start.Value.ToString("yyyy-MM-dd"), end.Value.ToString("yyyy-MM-dd")));
            var retVal = await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(OrderSearchCacheRegion.CreateChangeToken());

                var result = await customerOrderStatisticService.CollectStatisticsAsync(start.Value, end.Value);
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
            var parameters = paymentRequestConverter.GetPaymentParameters(callback);
            var result = await customerOrderPaymentService.PostProcessPaymentAsync(parameters);

            var (response, succeeded) = paymentRequestConverter.GetResponse(result);

            return succeeded
                ? Ok(response)
                : BadRequest(response);
        }

        /// <summary>
        /// Payment callback operation used by external payment services to inform post process payment in our system
        /// </summary>
        [HttpPost]
        [Route("~/api/paymentcallback-raw")]
        public async Task<ActionResult<PostProcessPaymentRequestResult>> PostProcessPaymentRaw()
        {
            var parameters = paymentRequestConverter.GetPaymentParameters(await GetRequestBody(), Request.QueryString.Value);
            var result = await customerOrderPaymentService.PostProcessPaymentAsync(parameters);

            var (response, succeeded) = paymentRequestConverter.GetResponse(result);

            return succeeded
                ? Ok(response)
                : BadRequest(response);
        }

        private async Task<string> GetRequestBody()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        [HttpGet]
        [Route("invoice/{orderNumber}")]
        [SwaggerFileResponse]
        public async Task<ActionResult> GetInvoicePdf(string orderNumber)
        {
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Number = orderNumber;
            searchCriteria.Take = 1;

            var orders = await searchService.SearchAsync(searchCriteria);
            var order = orders.Results.FirstOrDefault();

            if (order == null)
            {
                throw new InvalidOperationException($"Cannot find order with number {orderNumber}");
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var notification = await notificationSearchService.GetNotificationAsync<InvoiceEmailNotification>(new TenantIdentity(order.StoreId, nameof(Store)));
            notification.CustomerOrder = order;
            notification.LanguageCode = order.LanguageCode;
            var message = AbstractTypeFactory<EmailNotificationMessage>.TryCreateInstance($"{notification.Kind}Message");
            await notification.ToMessageAsync(message, notificationTemplateRenderer);

            if (message.Body.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Document could not be rendered because InvoiceEmailNotification template is empty.");
            }

            var result = GeneratePdf(message.Body);

            return new FileContentResult(result, "application/pdf");
        }

        [HttpGet]
        [Route("{id}/changes")]
        public async Task<ActionResult<OperationLog[]>> GetOrderChanges(string id)
        {
            var result = Array.Empty<OperationLog>();
            var order = await customerOrderService.GetByIdAsync(id);
            if (order != null)
            {
                var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
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
                result = (await changeLogSearchService.SearchAsync(criteria)).Results.ToArray();

            }
            return Ok(result);
        }


        [HttpPost]
        [Route("searchChanges")]
        public async Task<ActionResult<ChangeLogSearchResult>> SearchOrderChanges([FromBody] CustomerOrderHistorySearchCriteria historySearchCriteria)
        {
            if (historySearchCriteria.OrderId == null)
            {
                throw new InvalidOperationException("Order ID can not be null");
            }

            var order = await customerOrderService.GetByIdAsync(historySearchCriteria.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {historySearchCriteria.OrderId}");
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
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

            var result = await changeLogSearchService.SearchAsync(criteria);
            return Ok(result);
        }

        [HttpGet]
        [Route("indexed/searchEnabled")]
        public ActionResult GetOrderFullTextSearchEnabled()
        {
            var result = configuration.IsOrderFullTextSearchEnabled();
            return Ok(new { Result = result });
        }

        [HttpPost]
        [Route("indexed/search")]
        public async Task<ActionResult<CustomerOrderSearchResult>> SearchCustomerOrderIndexed([FromBody] CustomerOrderIndexedSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await indexedSearchService.SearchCustomerOrdersAsync(criteria);
            return Content(JsonConvert.SerializeObject(result, outputJsonSerializerSettings.Value), "application/json");
        }

        private byte[] GeneratePdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = {
                    PaperSize = EnumUtility.SafeParse(htmlToPdfOptions.Value.PaperSize, PaperKind.A4),
                    ViewportSize = htmlToPdfOptions.Value.ViewportSize,
                    DPI = htmlToPdfOptions.Value.DPI
                },
                Objects = {
                    new ObjectSettings {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = htmlToPdfOptions.Value.DefaultEncoding, MinimumFontSize = htmlToPdfOptions.Value.MinimumFontSize },
                    }
                }
            };
            var result = converter.Convert(doc);

            return result;
        }

        /// <summary>
        /// Partial update for the specified Orde by id
        /// </summary>
        /// <param name="id">Orde id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatcOrder(string id, [FromBody] JsonPatchDocument<CustomerOrder> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var order = await customerOrderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(order, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResult = await ValidateAsync(order);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    validationResult.Errors
                });
            }

            try
            {
                await customerOrderService.SaveChangesAsync([order]);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return NoContent();
        }

        private async Task<ValidationResult> ValidateAsync(CustomerOrder customerOrder)
        {
            if (await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.CustomerOrderValidation))
            {
                return await customerOrderValidator.ValidateAsync(customerOrder);
            }

            return new ValidationResult();
        }

        private static string GetNoStoreErrorMessage(CustomerOrder order)
        {
            return $"Store {order.StoreId} does not exist in the system. Please create {order.StoreId} store or select another store for the order.";
        }
    }
}
