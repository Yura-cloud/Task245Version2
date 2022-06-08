using System;
using System.Collections.Generic;
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

        private readonly ILogger<CreateOrdersService> _logger;

        private readonly IFtpDownLoaderService _ftpDownLoaderService;

        private LinnworksMacroBase LinnWorks { get; }

        public CreateOrdersService(ILogger<CreateOrdersService> logger,
            IFtpDownLoaderService ftpDownLoaderService)
        {
            _logger = logger;
            _ftpDownLoaderService = ftpDownLoaderService;
            LinnWorks = new LinnworksMacroBase();
        }

        public Guid? PullOrdersFromWasp(string locationName, IConfiguration configuration, string token)
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
            var ordersLines = _ftpDownLoaderService.GetRowsOfOrders();
            if (!ordersLines.Any())
            {
                _logger.LogWarning("**WASP server does not have any orders**");
                return FulfilmentCenter;
            }

            var channelOrders = ordersLines.Select(ChannelOrderMapper.MapOrdersRowsToChannelOrders).ToList();
            CreateOpenOrders(channelOrders);
            return FulfilmentCenter;
        }

        private void CreateOpenOrders(List<ChannelOrder> channelOrders)
        {
            try
            {
                LinnWorks.Api.Orders.CreateOrders(channelOrders, LocationName);
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while CreateOrdersCustom, with message {e.Message}**");
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
                _logger.LogError($"**Failed while GetStockLocations() with, message{e.Message}**");
                return new List<InventoryStockLocation>();
            }
        }
    }
}