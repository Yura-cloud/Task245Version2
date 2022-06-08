using System;
using Microsoft.Extensions.Configuration;

namespace WaspIntegration.Service.Interfaces
{
    public interface IOrderService
    {
        Guid? PullOrdersFromWasp(string locationName, IConfiguration configuration, string token);
    }
}