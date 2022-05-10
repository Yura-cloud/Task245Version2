using LinnworksAPI;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using LinnworksMacroHelpers.Helpers;
using WaspIntegration.Business.Services.Mappers;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class CreateOrderService : IOrderService
    {
        private Guid FulfilmentCenter { get; set; }

        private readonly ILogger<CreateOrderService> _logger;

        private readonly IFtpDownLoaderService _ftpDownLoaderService;

        private const int ItemQuantity = 1;

        private LinnworksMacroBase LinnWorks { get; }

        public CreateOrderService(ILogger<CreateOrderService> logger, IFtpDownLoaderService ftpDownLoaderService)
        {
            _logger = logger;
            _ftpDownLoaderService = ftpDownLoaderService;
            LinnWorks = new LinnworksMacroBase();
        }

        public Guid? PullOrders(string locationName, IConfiguration configuration, string token)
        {
            LinnWorks.Api = InitializeHelper.GetApiManagerForPullOrders(configuration, token);

            var locationId = GetLocationId(locationName);
            if (locationId == null)
            {
                _logger.LogInformation($"**There is no location name with the given parameter => {locationName}**");
                return null;
            }

            FulfilmentCenter = locationId.Value;

            var ordersLines = _ftpDownLoaderService.GetRowsOfOrders();

            if (!ordersLines.Any())
            {
                _logger.LogWarning("**WASP server does not have any orders**");
                return FulfilmentCenter;
            }

            var openOrders = CreateOpenedOrders(ordersLines);

            if (openOrders.Count == 0)
            {
                return FulfilmentCenter;
            }

            foreach (var order in openOrders)
            {
                SetOrderCustomerInfo(order.OrderId, order.CustomerInfo);
                SetOrderShippingInfo(order.OrderId, MapToUpdateOrderShippingInfoRequest(order.ShippingInfo));
                SetOrderGeneralInfo(order.OrderId, order.GeneralInfo);
            }

            AddOrdersItems(openOrders);

            foreach (var id in openOrders.Select(o => o.OrderId))
            {
                SetOrderExtendedProperty(id);
            }

            return FulfilmentCenter;
        }

        private void SetOrderExtendedProperty(Guid orderId)
        {
            try
            {
                var request = new AddExtendedPropertiesRequest
                {
                    OrderId = orderId,
                    ExtendedProperties = new[]
                    {
                        new BasicExtendedProperty
                        {
                            Name = "Dispatched",
                            Type = "Attribute",
                            Value = "NoAction"
                        }
                    }
                };

                LinnWorks.Api.Orders.AddExtendedProperties(request);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"**Failed while AddExtendedProperties, for this OrderId => {orderId}, with message {e.Message}**");
            }
        }

        private void AddOrdersItems(List<OpenOrder> openOrders)
        {
            foreach (var order in openOrders)
            {
                var itemNumber = order.Items.FirstOrDefault()?.ItemNumber;
                var itemId = GetItemIdBySku(itemNumber);

                if (itemId != null)
                {
                    AddOrderItem(order.OrderId, itemId.Value, itemNumber);
                    continue;
                }

                _logger.LogWarning($"**Item of this Order => {order.NumOrderId}, was not find**");
            }
        }


        private UpdateOrderShippingInfoRequest MapToUpdateOrderShippingInfoRequest(OrderShippingInfo info)
        {
            var infoRequest = new UpdateOrderShippingInfoRequest
            {
                PostalServiceId = GetPostalService(info.Vendor),
                TrackingNumber = info.TrackingNumber,
                TotalWeight = info.TotalWeight
            };

            return infoRequest;
        }

        private Guid? GetPostalService(string vendorName)
        {
            var postalServices = GetPostalServices();

            var service = postalServices.FirstOrDefault(ps =>
                string.Equals(ps.Vendor, vendorName, StringComparison.CurrentCultureIgnoreCase) ||
                string.Equals(ps.PostalServiceName, vendorName, StringComparison.CurrentCultureIgnoreCase));

            if (service != null)
            {
                return service.id;
            }

            _logger.LogWarning($"**There is no PostalService with this name -> {vendorName}**");
            return null;
        }

        private List<OpenOrder> CreateOpenedOrders(IEnumerable<string> ordersLines)
        {
            var openOrders = new List<OpenOrder>();
            foreach (var line in ordersLines)
            {
                if (DuplicateOrder(line.Split(';')[3], line.Split(';')[4]))
                {
                    continue;
                }

                var newOrder = CreateOrder();
                if (newOrder.OrderId != Guid.Empty)
                {
                    OpenOrderMapper.MapToOpenOrder(line.Split(';'), newOrder);
                }
                else
                {
                    _logger.LogWarning(
                        $"**Order with this ReferenceNum=> {newOrder.GeneralInfo.ReferenceNum}, was not created**");
                }

                openOrders.Add(newOrder);
            }

            return openOrders;
        }

        private bool DuplicateOrder(string referenceNumber, string receivedDate)
        {
            var filters = new FieldsFilter
            {
                TextFields = new List<TextFieldFilter>
                {
                    new TextFieldFilter
                    {
                        FieldCode = FieldCode.GENERAL_INFO_REFERENCE_NUMBER,
                        Text = referenceNumber,
                        Type = TextFieldFilterType.Equal
                    }
                },
                DateFields = new List<DateFieldFilter>
                {
                    new DateFieldFilter
                    {
                        FieldCode = FieldCode.GENERAL_INFO_DATE,
                        DateFrom = Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(receivedDate)),
                        DateTo = Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(receivedDate))
                    }
                }
            };
            try
            {
                var openOrders = LinnWorks.Api.Orders.GetAllOpenOrders(filters, null, FulfilmentCenter,
                    null, true);
                return openOrders.Any();
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while GetAllOpenOrders, whit message {e.Message}**");
                return true;
            }
        }

        private Guid? GetLocationId(string locationName)
        {
            var stockLocations = GetStockLocations();

            var stockLocation = stockLocations.FirstOrDefault(l =>
                string.Equals(l.LocationName, locationName, StringComparison.CurrentCultureIgnoreCase));

            return stockLocation?.StockLocationId;
        }

        private List<PostalService_WithChannelAndShippingLinks> GetPostalServices()
        {
            try
            {
                return LinnWorks.Api.PostalServices.GetPostalServices();
            }
            catch (Exception ex)
            {
                _logger.LogError($"**Failed while retrieving PostalServices, with message: {ex.Message}**");

                return new List<PostalService_WithChannelAndShippingLinks>();
            }
        }

        private Guid? GetItemIdBySku(string sku)
        {
            try
            {
                var res = LinnWorks.Api.Inventory.GetStockItemIdsBySKU(new GetStockItemIdsBySKURequest
                {
                    SKUS = new List<string> {sku}
                });

                if (res.Items.Count > 0)
                {
                    return res.Items[0].StockItemId;
                }

                _logger.LogWarning($"**There isn't any item with SKU: {sku}**");
            }
            catch (Exception ex)
            {
                _logger.LogError($"**Failed while getting item by SKU with code: {ex.Message}**");
            }

            return null;
        }

        private void AddOrderItem(Guid orderId, Guid itemId, string itemNumber)
        {
            try
            {
                LinnWorks.Api.Orders.AddOrderItem(orderId, itemId, itemNumber, FulfilmentCenter, ItemQuantity,
                    new LinePricingRequest());
            }
            catch (Exception ex)
            {
                _logger.LogError($"**Failed while adding item to this Order -> {orderId}: {ex.Message}**");
            }
        }

        private OpenOrder CreateOrder()
        {
            try
            {
                var order = LinnWorks.Api.Orders.CreateNewOrder(FulfilmentCenter, false);
                return order ?? new OpenOrder();
            }
            catch (Exception ex)
            {
                _logger.LogError($"**Failed while creating new open order with code: {ex.Message}**");
                return new OpenOrder();
            }
        }

        private void SetOrderGeneralInfo(Guid orderId, OrderGeneralInfo orderInfo)
        {
            try
            {
                LinnWorks.Api.Orders.SetOrderGeneralInfo(orderId, orderInfo, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"**Failed while updating this order -> {orderId}, general info with code: {ex.Message}**");
            }
        }

        private void SetOrderCustomerInfo(Guid orderId, OrderCustomerInfo orderInfo)
        {
            try
            {
                LinnWorks.Api.Orders.SetOrderCustomerInfo(orderId, orderInfo, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"**Failed while updating this order -> {orderId}, customer info with code: {ex.Message}**");
            }
        }

        private void SetOrderShippingInfo(Guid orderId, UpdateOrderShippingInfoRequest infoRequest)
        {
            try
            {
                LinnWorks.Api.Orders.SetOrderShippingInfo(orderId, infoRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"**Failed while updating this order -> {orderId}, shipping info with code: {ex.Message}**");
            }
        }

        private IEnumerable<InventoryStockLocation> GetStockLocations()
        {
            try
            {
                return LinnWorks.Api.Inventory.GetStockLocations();
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while GetStockLocations() with, message{e.Message}**");
                return new List<InventoryStockLocation>();
            }
        }
    }
}