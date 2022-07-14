using System.Collections.Generic;
using LinnworksAPI;

namespace WaspIntegration.Service.Interfaces
{
    public interface IPriceService
    {
       Dictionary<string, double> GetItemsPrices(List<string> skus, ApiObjectManager apiObjectManager);
    }
}