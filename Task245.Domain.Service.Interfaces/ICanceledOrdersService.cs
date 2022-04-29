using System.Collections.Generic;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Configuration;
using WaspIntegration.Domain;

namespace WaspIntegration.Service.Interfaces
{
    public interface ICanceledOrdersService
    {
        void ParkingCanceledOrders(string emailText, IConfiguration configuration, string token, string location);
        List<ManifestOrderInfoModel> ManifestInfos { get; set; }
        LinnworksMacroBase LinnWorks { get; set; }
    }
}