using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspAPI.Controllers
{
    [ApiController]
    [Route("api/cancelOrder")]
    public class CanceledOrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CanceledOrdersController> _logger;
        private readonly IMailWaspService _mailWaspService;
        private readonly ICanceledOrdersService _canceledOrdersService;

        public CanceledOrdersController(IConfiguration configuration, ILogger<CanceledOrdersController> logger,
            IMailWaspService mailWaspService, ICanceledOrdersService canceledOrdersService)
        {
            _configuration = configuration;
            _logger = logger;
            _mailWaspService = mailWaspService;
            _canceledOrdersService = canceledOrdersService;
        }

        [HttpGet]
        public IActionResult CancelOrders(string token, string email, string appPassword,string location, string subject,
            string supplierCode)
        {
            try
            {
                var mailText = _mailWaspService.ReadInboxLetters(email, appPassword, subject);
                if (string.IsNullOrEmpty(mailText))
                {
                    return Ok();
                }

                _canceledOrdersService.ParkingCanceledOrders(mailText, _configuration, token,location,supplierCode);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while using CanselOrdersController, with message {e}");
                return BadRequest();
            }

            return Ok();
        }
    }
}