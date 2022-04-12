using System;
using Microsoft.Extensions.Configuration;

namespace WaspIntegration.Service.Interfaces
{
    public interface IOrderService
    {
        Guid? PullOrders(string locationName, IConfiguration configuration, string token);
    }
}