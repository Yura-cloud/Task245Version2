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
        private readonly IMailService _mailService;
        private readonly ICanceledOrdersService _canceledOrdersService;

        public CanceledOrdersController(IConfiguration configuration, ILogger<CanceledOrdersController> logger,
            IMailService mailService, ICanceledOrdersService canceledOrdersService)
        {
            _configuration = configuration;
            _logger = logger;
            _mailService = mailService;
            _canceledOrdersService = canceledOrdersService;
        }

        [HttpGet]
        public IActionResult CanselOrders(string token, string email, string appPassword,string location)
        {
            try
            {
                var mailText = _mailService.ReadInboxLetters(email, appPassword);
                if (string.IsNullOrEmpty(mailText))
                {
                    _logger.LogInformation("**Mail does not have any unread messages**");
                    return Ok();
                }

                _canceledOrdersService.ParkedCanceledOrders(mailText, _configuration, token,location);
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