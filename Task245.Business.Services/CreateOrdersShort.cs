using System;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class CreateOrdersShort : IOrderService
    {
        private Guid FulfilmentCenter { get; set; }
        private readonly IFtpServerService _ftpServerService;
        private readonly ILogger<CreateOrdersShort> _logger;
        private LinnworksMacroBase LinnWorks { get; }

        public CreateOrdersShort(IFtpServerService ftpServerService, ILogger<CreateOrdersShort> logger)
        {
            _ftpServerService = ftpServerService;
            _logger = logger;
            LinnWorks = new LinnworksMacroBase();
        }

        public Guid? PullOrders(string locationName, IConfiguration configuration, string token)
        {
            throw new NotImplementedException();
        }
    }
}