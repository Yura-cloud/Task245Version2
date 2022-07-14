using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinnworksAPI;
using LinnworksMacroHelpers;
using LinnworksMacroHelpers.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Business.Services.Mappers;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class CreateOrdersService : IOrderService
    {
        private Guid FulfilmentCenter { get; set; }
        private string LocationName { get; set; }

        private const int ItemSKU = 9;

        private readonly ILogger<CreateOrdersService> _logger;

        private readonly IFtpDownLoaderService _ftpDownLoaderService;

        private readonly IPriceService _priceService;

        private LinnworksMacroBase LinnWorks { get; }

        public CreateOrdersService(ILogger<CreateOrdersService> logger, IFtpDownLoaderService ftpDownLoaderService,
            IPriceService priceService)
        {
            _logger = logger;
            _ftpDownLoaderService = ftpDownLoaderService;
            _priceService = priceService;
            LinnWorks = new LinnworksMacroBase();
        }

        public Guid? PullOrdersFromWasp(string locationName, IConfiguration configuration, string token)
        {
            LinnWorks.Api = InitializeHelper.GetApiManagerForPullOrders(configuration, token);
            LocationName = locationName;
            var locationId = GetLocationId(locationName);
            if (locationId == null)
            {
                _logger.LogError($"There is no location name with the given parameter => {locationName}");
                return null;
            }

            FulfilmentCenter = locationId.Value;
            var ordersLines = _ftpDownLoaderService.GetRowsOfOrders();
            // var ordersLines =
            //     File.ReadAllLines("C:\\Users\\Yura\\OneDrive\\Desktop\\Patternika\\Tasks\\245_Wasp\\R0404_orders.txt");
            if (!ordersLines.Any())
            {
                _logger.LogWarning("**WASP server does not have any orders**");
                return FulfilmentCenter;
            }

            var channelOrders = ordersLines.Select(ChannelOrderMapper.MapOrdersRowsToChannelOrders).ToList();
            var skus = ordersLines.Select(o => o.Split(';')[ItemSKU]).ToList();
            var itemsPrices = _priceService.GetItemsPrices(skus, LinnWorks.Api);
            if (itemsPrices.Count != 0)
            {
                AddPricesToChannelOrders(channelOrders, itemsPrices);
            }

            CreateOpenOrders(channelOrders);
            return FulfilmentCenter;
        }

        private void AddPricesToChannelOrders(List<ChannelOrder> channelOrders, Dictionary<string, double> itemsInfo)
        {
            foreach (var channelOrder in channelOrders)
            {
                var item = channelOrder.OrderItems.FirstOrDefault();
                if (item != null)
                {
                    itemsInfo.TryGetValue(item.ItemNumber, out var price);
                    item.PricePerUnit = price;
                }
            }
        }

        private void CreateOpenOrders(List<ChannelOrder> channelOrders)
        {
            try
            {
                LinnWorks.Api.Orders.CreateOrders(channelOrders, LocationName);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while CreateOrdersCustom, with message {e.Message}");
            }
        }

        private Guid? GetLocationId(string locationName)
        {
            var stockLocations = GetStockLocations();

            var stockLocation = stockLocations.FirstOrDefault(l =>
                string.Equals(l.LocationName, locationName, StringComparison.CurrentCultureIgnoreCase));

            return stockLocation?.StockLocationId;
        }

        private IEnumerable<InventoryStockLocation> GetStockLocations()
        {
            try
            {
                return LinnWorks.Api.Inventory.GetStockLocations();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while GetStockLocations() with, message{e.Message}");
                return new List<InventoryStockLocation>();
            }
        }
    }
}