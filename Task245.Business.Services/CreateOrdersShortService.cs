using System;
using System.Collections.Generic;
using System.Linq;
using LinnworksAPI;
using LinnworksMacroHelpers;
using LinnworksMacroHelpers.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class CreateOrdersShortService : IOrderService
    {
        private const string Source = "Wasp";
        private const string SubSource = "Studio";
        private Guid FulfilmentCenter { get; set; }
        private string LocationName { get; set; }
        private readonly IFtpServerService _ftpServerService;
        private readonly ILogger<CreateOrdersShortService> _logger;
        private LinnworksMacroBase LinnWorks { get; }

        public CreateOrdersShortService(IFtpServerService ftpServerService, ILogger<CreateOrdersShortService> logger)
        {
            _ftpServerService = ftpServerService;
            _logger = logger;
            LinnWorks = new LinnworksMacroBase();
        }

        public Guid? PullOrders(string locationName, IConfiguration configuration, string token)
        {
            LinnWorks.Api = InitializeHelper.GetApiManagerForPullOrders(configuration, token);
            LocationName = locationName;

            var locationId = GetLocationId(locationName);
            if (locationId == null)
            {
                _logger.LogInformation($"**There is no location name with the given parameter => {locationName}**");
                return null;
            }

            FulfilmentCenter = locationId.Value;

            //var ordersLines = _ftpServerService.GetLinesOfOrdersFromServer();
            var ordersLines = _ftpServerService.GetLinesOfOrdersFromLocalComputer();

            if (!ordersLines.Any())
            {
                _logger.LogWarning("**WASP server does not have any orders**");
                return FulfilmentCenter;
            }

            var channelOrders = new List<ChannelOrder>();
            foreach (var line in ordersLines)
            {
                var chanelOrder = MapOrdersLinesToChannelOrders(line);
                channelOrders.Add(chanelOrder);
            }

            CreateOpenOrders(channelOrders);

            return FulfilmentCenter;
        }

        private void CreateOpenOrders(List<ChannelOrder> channelOrders)
        {
            try
            {
                var s = LinnWorks.Api.Orders.CreateOrders(channelOrders, LocationName);
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while CreateOrdersCustom, with message {e.Message}**");
            }
        }

        private ChannelOrder MapOrdersLinesToChannelOrders(string orderLine)
        {
            var channelOrder = new ChannelOrder();
            var splitProperties = orderLine.Split(';');

            channelOrder.ChannelBuyerName = splitProperties[2];
            channelOrder.OrderIdentifierTags = new HashSet<string>();

            channelOrder.DeliveryAddress = new ChannelAddress
            {
                FullName = $"{splitProperties[16]} {splitProperties[17]}",
                Company = splitProperties[37],
                Address1 = splitProperties[29],
                Address2 = $"{splitProperties[30]} {splitProperties[31]}",
                Address3 = $"{splitProperties[32]} {splitProperties[33]}",
                Town = splitProperties[46],
                Region = splitProperties[14],
                PostCode = splitProperties[34],
                PhoneNumber = splitProperties[13]
            };

            channelOrder.ReferenceNumber = splitProperties[3];
            channelOrder.ReceivedDate =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(splitProperties[4]));
            channelOrder.SecondaryReferenceNumber = splitProperties[5];
            channelOrder.ExternalReference = splitProperties[6];
            channelOrder.DispatchBy =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(splitProperties[28]));
            channelOrder.Source = Source;
            channelOrder.SubSource = SubSource;
            channelOrder.PaymentStatus = PaymentStatus.Paid;
            channelOrder.PostalServiceName = splitProperties[11];

            channelOrder.ExtendedProperties = new List<ChannelOrderExtendedProperty>()
            {
                new ChannelOrderExtendedProperty
                {
                    Name = "Dispatched",
                    Type = "Attribute",
                    Value = "NoAction"
                },
                new ChannelOrderExtendedProperty
                {
                    Name = "HaulierCode",
                    Type = "Attribute",
                    Value = splitProperties[10]
                }
            };

            channelOrder.OrderItems = new List<ChannelOrderItem>
            {
                new ChannelOrderItem
                {
                    TaxCostInclusive = true,
                    UseChannelTax = true,
                    Qty = 1,
                    ItemNumber = splitProperties[7],
                    ChannelSKU = splitProperties[7],
                    IsService = false,
                    ItemTitle = splitProperties[8]
                }
            };

            return channelOrder;
        }

        private Guid? GetLocationId(string locationName)
        {
            var stockLocations = GetStockLocations();

            var stockLocation = stockLocations.FirstOrDefault(l =>
                string.Equals(l.LocationName, locationName, StringComparison.CurrentCultureIgnoreCase));

            return stockLocation?.StockLocationId;
        }

        private bool IsDuplicateOrder(ChannelOrder order)
        {
            try
            {
                var ordersDetails = LinnWorks.Api.Orders.GetOrderDetailsByReferenceId(order.ReferenceNumber);
                if (ordersDetails.Count == 0)
                {
                    return false;
                }

                return IsSameProperties(ordersDetails, order);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while GetOrderDetailsByReferenceId, whit error {e.Message}");
                return true;
            }
        }

        private bool IsSameProperties(List<OrderDetails> ordersDetails, ChannelOrder order)
        {
            ordersDetails = ordersDetails
                .Where(o => o.GeneralInfo.ReceivedDate == order.ReceivedDate)
                .Where(o => o.ExtendedProperties.FirstOrDefault(e => e.Name == "CompanyCode")?.Value ==
                            order.ExtendedProperties.FirstOrDefault(e => e.Name == "CompanyCode")?.Value)
                .ToList();

            return ordersDetails.Count > 0;
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