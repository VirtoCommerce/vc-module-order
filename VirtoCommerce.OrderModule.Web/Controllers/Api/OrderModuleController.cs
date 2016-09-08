using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using CacheManager.Core;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.OrderModule.Web.BackgroundJobs;
using VirtoCommerce.OrderModule.Web.Converters;
using VirtoCommerce.OrderModule.Web.Security;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Web.Security;
using VirtoCommerce.Platform.Data.Common;
using coreModel = VirtoCommerce.Domain.Order.Model;
using webModel = VirtoCommerce.OrderModule.Web.Model;

namespace VirtoCommerce.OrderModule.Web.Controllers.Api
{
    [RoutePrefix("api/order/customerOrders")]
    public class OrderModuleController : ApiController
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICustomerOrderSearchService _searchService;
        private readonly IUniqueNumberGenerator _uniqueNumberGenerator;
        private readonly IStoreService _storeService;
        private readonly ICacheManager<object> _cacheManager;
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly ISecurityService _securityService;
        private readonly IPermissionScopeService _permissionScopeService;
        private readonly ISettingsManager _settingManager;
        private static readonly object _lockObject = new object();

        public OrderModuleController(ICustomerOrderService customerOrderService, ICustomerOrderSearchService searchService, IStoreService storeService, IUniqueNumberGenerator numberGenerator, ICacheManager<object> cacheManager, Func<IOrderRepository> repositoryFactory, IPermissionScopeService permissionScopeService, ISecurityService securityService, ISettingsManager settingManager)
        {
            _customerOrderService = customerOrderService;
            _searchService = searchService;
            _uniqueNumberGenerator = numberGenerator;
            _storeService = storeService;
            _cacheManager = cacheManager;
            _repositoryFactory = repositoryFactory;
            _securityService = securityService;
            _permissionScopeService = permissionScopeService;
            _settingManager = settingManager;
        }

        /// <summary>
        /// Search customer orders by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        [ResponseType(typeof(webModel.SearchResult))]
        public IHttpActionResult Search(coreModel.SearchCriteria criteria)
        {
            //Scope bound ACL filtration
            criteria = FilterOrderSearchCriteria(HttpContext.Current.User.Identity.Name, criteria);

            var retVal = _searchService.Search(criteria);
            return Ok(retVal.ToWebModel());
        }

        /// <summary>
        /// Find customer order by number
        /// </summary>
        /// <remarks>Return a single customer order with all nested documents or null if order was not found</remarks>
        /// <param name="number">customer order number</param>
        [HttpGet]
        [Route("number/{number}")]
        [ResponseType(typeof(webModel.CustomerOrder))]
        public IHttpActionResult GetByNumber(string number)
        {
            var retVal = _customerOrderService.GetByOrderNumber(number, coreModel.CustomerOrderResponseGroup.Full);
            if (retVal == null)
            {
                return NotFound();
            }

            //Scope bound security check
            var scopes = _permissionScopeService.GetObjectPermissionScopeStrings(retVal).ToArray();
            if (!_securityService.UserHasAnyPermission(User.Identity.Name, scopes, OrderPredefinedPermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var result = retVal.ToWebModel();
            //Set scopes for UI scope bounded ACL checking
            result.Scopes = scopes;

            return Ok(result);
        }


        /// <summary>
        /// Find customer order by id
        /// </summary>
        /// <remarks>Return a single customer order with all nested documents or null if order was not found</remarks>
        /// <param name="id">customer order id</param>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(webModel.CustomerOrder))]
        public IHttpActionResult GetById(string id)
        {
            var retVal = _customerOrderService.GetById(id, coreModel.CustomerOrderResponseGroup.Full);
            if (retVal == null)
            {
                return NotFound();
            }

            //Scope bound security check
            var scopes = _permissionScopeService.GetObjectPermissionScopeStrings(retVal).ToArray();
            if (!_securityService.UserHasAnyPermission(User.Identity.Name, scopes, OrderPredefinedPermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var result = retVal.ToWebModel();
            //Set scopes for UI scope bounded ACL checking
            result.Scopes = scopes;

            return Ok(result);
        }

        /// <summary>
        /// Create new customer order based on shopping cart.
        /// </summary>
        /// <param name="id">shopping cart id</param>
        [HttpPost]
        [Route("{id}")]
        [ResponseType(typeof(webModel.CustomerOrder))]
        [CheckPermission(Permission = OrderPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrderFromCart(string id)
        {
            var retVal = _customerOrderService.CreateByShoppingCart(id);
            return Ok(retVal.ToWebModel());
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
        [ResponseType(typeof(webModel.ProcessPaymentResult))]
        public IHttpActionResult ProcessOrderPayments(string orderId, string paymentId, [SwaggerOptional] BankCardInfo bankCardInfo)
        {
            //search first by order number
            var order = _customerOrderService.GetByOrderNumber(orderId, coreModel.CustomerOrderResponseGroup.Full);

            //if not found by order number search by order id
            if (order == null)
                order = _customerOrderService.GetById(orderId, coreModel.CustomerOrderResponseGroup.Full);

            if (order == null)
            {
                throw new NullReferenceException("order");
            }
            var payment = order.InPayments.FirstOrDefault(x => x.Id == paymentId);
            if (payment == null)
            {
                throw new NullReferenceException("payment");
            }
            var store = _storeService.GetById(order.StoreId);
            var paymentMethod = store.PaymentMethods.FirstOrDefault(x => x.Code == payment.GatewayCode);
            if (paymentMethod == null)
            {
                throw new NullReferenceException("appropriate paymentMethod not found");
            }

            var context = new ProcessPaymentEvaluationContext
            {
                Order = order,
                Payment = payment,
                Store = store,
                BankCardInfo = bankCardInfo
            };

            var result = paymentMethod.ProcessPayment(context);

            _customerOrderService.Update(new[] { order });

            var retVal = new webModel.ProcessPaymentResult();
            retVal.InjectFrom(result);
            retVal.PaymentMethodType = paymentMethod.PaymentMethodType;

            return Ok(retVal);
        }

        /// <summary>
        /// Add new customer order to system
        /// </summary>
        /// <param name="customerOrder">customer order</param>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(webModel.CustomerOrder))]
        [CheckPermission(Permission = OrderPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrder(webModel.CustomerOrder customerOrder)
        {
            var retVal = _customerOrderService.Create(customerOrder.ToCoreModel());
            return Ok(retVal.ToWebModel());
        }

        /// <summary>
        ///  Update a existing customer order 
        /// </summary>
        /// <param name="customerOrder">customer order</param>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Update(webModel.CustomerOrder customerOrder)
        {
            var coreOrder = customerOrder.ToCoreModel();

            //Check scope bound permission
            var scopes = _permissionScopeService.GetObjectPermissionScopeStrings(coreOrder).ToArray();
            if (!_securityService.UserHasAnyPermission(User.Identity.Name, scopes, OrderPredefinedPermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            _customerOrderService.Update(new[] { coreOrder });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get new shipment for specified customer order
        /// </summary>
        /// <remarks>Return new shipment document with populates all required properties.</remarks>
        /// <param name="id">customer order id </param>
        [HttpGet]
        [Route("{id}/shipments/new")]
        [ResponseType(typeof(webModel.Shipment))]
        public IHttpActionResult GetNewShipment(string id)
        {
            var order = _customerOrderService.GetById(id, coreModel.CustomerOrderResponseGroup.Full);
            if (order != null)
            {
                var retVal = new coreModel.Shipment
                {
                    Id = Guid.NewGuid().ToString(),
                    Currency = order.Currency
                };
                var numberTemplate = _settingManager.GetValue("Order.ShipmentNewNumberTemplate", "SH{0:yyMMdd}-{1:D5}");
                retVal.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate);

                //Detect not whole shipped items
                //TODO: LineItem partial shipping
                var shippedLineItemIds = order.Shipments.SelectMany(x => x.Items).Select(x => x.LineItemId);

                //TODO Add check for digital products (don't add to shipment)
                retVal.Items = order.Items.Where(x => !shippedLineItemIds.Contains(x.Id))
                              .Select(x => new coreModel.ShipmentItem(x)).ToList();
                return Ok(retVal.ToWebModel());
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
        [ResponseType(typeof(webModel.PaymentIn))]
        public IHttpActionResult GetNewPayment(string id)
        {
            var order = _customerOrderService.GetById(id, coreModel.CustomerOrderResponseGroup.Full);
            if (order != null)
            {
                var retVal = new coreModel.PaymentIn
                {
                    Id = Guid.NewGuid().ToString(),
                    Currency = order.Currency,
                    CustomerId = order.CustomerId
                };
                var numberTemplate = _settingManager.GetValue("Order.PaymentInNewNumberTemplate", "PI{0:yyMMdd}-{1:D5}");
                retVal.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate);
                return Ok(retVal.ToWebModel());
            }

            return NotFound();
        }

        /// <summary>
        ///  Delete a whole customer orders
        /// </summary>
        /// <param name="ids">customer order ids for delete</param>
        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = OrderPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteOrdersByIds([FromUri] string[] ids)
        {
            _customerOrderService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        ///  Delete a concrete customer order operation (document) 
        /// </summary>
        /// <param name="id">customer order id</param>
        /// <param name="operationId">operation id</param>
        [HttpDelete]
        [Route("~/api/order/customerOrders/{id}/operations/{operationId}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(string id, string operationId)
        {
            var order = _customerOrderService.GetById(id, coreModel.CustomerOrderResponseGroup.Full);
            if (order != null)
            {
                var operation = order.GetFlatObjectsListWithInterface<coreModel.IOperation>().FirstOrDefault(x => ((Entity)x).Id == operationId);
                if (operation != null)
                {
                    var shipment = operation as coreModel.Shipment;
                    var payment = operation as coreModel.PaymentIn;
                    if (shipment != null)
                    {
                        order.Shipments.Remove(shipment);
                    }
                    else if (payment != null)
                    {
                        //If payment not belong to order need remove payment in shipment
                        if (!order.InPayments.Remove(payment))
                        {
                            var paymentContainsShipment = order.Shipments.FirstOrDefault(x => x.InPayments.Contains(payment));
                            paymentContainsShipment.InPayments.Remove(payment);
                        }
                    }
                }
                _customerOrderService.Update(new[] { order });
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        ///  Get a some order statistic information for Commerce manager dashboard
        /// </summary>
        /// <param name="start">start interval date</param>
        /// <param name="end">end interval date</param>
        [HttpGet]
        [Route("~/api/order/dashboardStatistics")]
        [ResponseType(typeof(webModel.DashboardStatisticsResult))]
        [OverrideAuthorization]
        public IHttpActionResult GetDashboardStatistics([FromUri]DateTime? start = null, [FromUri]DateTime? end = null)
        {
            webModel.DashboardStatisticsResult retVal;
            start = start ?? DateTime.UtcNow.AddYears(-1);
            end = end ?? DateTime.UtcNow;

            // Hack: to compinsate for incorrect Local dates to UTC
            end = end.Value.AddDays(2);
            var cacheKey = String.Join(":", "Statistic", start.Value.ToString("yyyy-MM-dd"), end.Value.ToString("yyyy-MM-dd"));
            lock (_lockObject)
            {
                retVal = _cacheManager.Get(cacheKey, "OrderModuleRegion", () =>
                {

                    var collectStaticJob = new CollectOrderStatisticJob(_repositoryFactory, _cacheManager);
                    return collectStaticJob.CollectStatistics(start.Value, end.Value);

                });
            }
            return Ok(retVal);
        }

        /// <summary>
        /// Payment callback operation used by external payment services to inform post process payment in our system
        /// </summary>
        /// <param name="callback">payment callback parameters</param>
        [HttpPost]
        [Route("~/api/paymentcallback")]
        [ResponseType(typeof(PostProcessPaymentResult))]
        public IHttpActionResult PostProcessPayment(webModel.PaymentCallbackParameters callback)
        {
            if (callback != null && callback.Parameters != null && callback.Parameters.Any(param => param.Key.EqualsInvariant("orderid")))
            {
                var orderId = callback.Parameters.First(param => param.Key.EqualsInvariant("orderid")).Value;
                //some payment method require customer number to be passed and returned. First search customer order by number
                var order = _customerOrderService.GetByOrderNumber(orderId, coreModel.CustomerOrderResponseGroup.Full);

                //if order not found by order number search by order id
                if (order == null)
                    order = _customerOrderService.GetById(orderId, coreModel.CustomerOrderResponseGroup.Full);

                var store = _storeService.GetById(order.StoreId);
                var parameters = new NameValueCollection();
                foreach (var param in callback.Parameters)
                {
                    parameters.Add(param.Key, param.Value);
                }
                var paymentMethod = store.PaymentMethods.Where(x => x.IsActive).FirstOrDefault(x => x.ValidatePostProcessRequest(parameters).IsSuccess);
                if (paymentMethod != null)
                {
                    var paymentOuterId = paymentMethod.ValidatePostProcessRequest(parameters).OuterId;

                    var payment = order.InPayments.FirstOrDefault(x => string.IsNullOrEmpty(x.OuterId) || x.OuterId == paymentOuterId);
                    if (payment == null)
                    {
                        throw new NullReferenceException("appropriate paymentMethod not found");
                    }

                    var context = new PostProcessPaymentEvaluationContext
                    {
                        Order = order,
                        Payment = payment,
                        Store = store,
                        OuterId = paymentOuterId,
                        Parameters = parameters
                    };

                    var retVal = paymentMethod.PostProcessPayment(context);
                    if (retVal != null)
                    {
                        _customerOrderService.Update(new[] { order });
                    }

                    // order Number is required
                    retVal.OrderId = order.Number;
                    return Ok(retVal);
                }
            }
            return Ok(new PostProcessPaymentResult { ErrorMessage = "cancel payment" });
        }


        private coreModel.SearchCriteria FilterOrderSearchCriteria(string userName, coreModel.SearchCriteria criteria)
        {

            if (!_securityService.UserHasAnyPermission(userName, null, OrderPredefinedPermissions.Read))
            {
                //Get defined user 'read' permission scopes
                var readPermissionScopes = _securityService.GetUserPermissions(userName)
                                                      .Where(x => x.Id.StartsWith(OrderPredefinedPermissions.Read))
                                                      .SelectMany(x => x.AssignedScopes)
                                                      .ToList();

                //Check user has a scopes
                //Stores
                criteria.StoreIds = readPermissionScopes.OfType<OrderStoreScope>()
                                                         .Select(x => x.Scope)
                                                         .Where(x => !String.IsNullOrEmpty(x))
                                                         .ToArray();

                var responsibleScope = readPermissionScopes.OfType<OrderResponsibleScope>().FirstOrDefault();
                //employee id
                if (responsibleScope != null)
                {
                    criteria.EmployeeId = userName;
                }
            }
            return criteria;
        }
    }
}
