using System;
using System.Collections.Generic;
using System.Linq;
using LinnworksAPI;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class PriceService : IPriceService
    {
        private readonly ILogger<PriceService> _logger;

        private LinnworksMacroBase LinnWorks { get; }

        public PriceService(ILogger<PriceService> logger)
        {
            _logger = logger;
            LinnWorks = new LinnworksMacroBase();
        }

        public Dictionary<string, double> GetItemsPrices(List<string> skus, ApiObjectManager apiObjectManager)
        {
            LinnWorks.Api = apiObjectManager;
            var itemsIds = GetItemsIds(skus);
            if (itemsIds.Count == 0)
            {
                return new Dictionary<string, double>();
            }

            var itemsInfo = GetItemsInfo(itemsIds.Select(i => i.StockItemId).ToList());
            if (itemsInfo.Count == 0)
            {
                return new Dictionary<string, double>();
            }

            var itemsWithPrice = new Dictionary<string, double>();
            foreach (var stockItemFullExtended in itemsInfo)
            {
                itemsWithPrice[stockItemFullExtended.ItemNumber] = stockItemFullExtended.RetailPrice ?? 0;
            }

            return itemsWithPrice;
        }

        private List<StockItemFullExtended> GetItemsInfo(List<Guid> ids)
        {
            try
            {
                return LinnWorks.Api.Stock.GetStockItemsFullByIds(new GetStockItemsFullByIdsRequest()
                    {
                        StockItemIds = ids,
                        DataRequirements = new List<StockItemFullExtendedDataRequirement>()
                    })
                    .StockItemsFullExtended;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while working with GetItemsInfo, with message {e.Message}");
            }

            return new List<StockItemFullExtended>();
        }

        private List<GetStockItemIdsBySKUItem> GetItemsIds(List<string> skus)
        {
            try
            {
                var request = new GetStockItemIdsBySKURequest {SKUS = skus};
                return LinnWorks.Api.Inventory.GetStockItemIdsBySKU(request).Items;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while working with GetStockItemIdsBySKU, with message {e.Message}");
            }

            return new List<GetStockItemIdsBySKUItem>();
        }
    }
}