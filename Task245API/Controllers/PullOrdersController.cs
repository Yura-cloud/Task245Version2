using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class PullOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IManifestService _manifestService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PullOrdersController> _logger;

        public PullOrdersController(IOrderService orderService, ILogger<PullOrdersController> logger,
            IManifestService manifestService, IConfiguration configuration)
        {
            _orderService = orderService;
            _logger = logger;
            _manifestService = manifestService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult PullOrdersFromWasp(string locationName, string token)
        {
            try
            {
                var locationId = _orderService.PullOrdersFromWasp(locationName, _configuration, token);

                _manifestService.UploadManifest(locationId, _configuration, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed while using PullOrdersController, with message {ex}");
                return BadRequest();
            }

            return Ok();
        } 
    }
}