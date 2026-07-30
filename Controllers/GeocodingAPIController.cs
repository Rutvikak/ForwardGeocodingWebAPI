using System;
using GeocodingAPI.Data;
using GeocodingAPI.Models;
using GeocodingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace GeocodingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeocodingAPIController : ControllerBase
    {
        private IGeocodingServices _geoService;
        private readonly GeocodingAPIDbContext _dbContext;
        private ILogger<GeocodingAPIController> _logger;
        public GeocodingAPIController(IGeocodingServices geoService, GeocodingAPIDbContext dbcontext, ILogger<GeocodingAPIController> logger)
        {
            _geoService = geoService;
            _dbContext = dbcontext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IResult> SearchGeoCoding([FromQuery] string address)
        {
            _logger.LogInformation($"Calling ValidationProcessing() by passing {address}");
            var response = await _geoService.ValidationProcessing(address);
            _logger.LogInformation("Call end ValidationProcessing()");
            return response;

        }
    }
}
