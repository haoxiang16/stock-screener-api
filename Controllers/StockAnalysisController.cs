using Microsoft.AspNetCore.Mvc;
using StockAPI.Interfaces;
using StockAPI.Models.DTOs;

namespace StockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockAnalysisController : ControllerBase
    {
        private readonly IStockAnalysisService _service;

        public StockAnalysisController(IStockAnalysisService service)
        {
            _service = service;
        }

        [HttpGet("growing-eps")]
        public async Task<ActionResult<IEnumerable<CompanyGrowthDTO>>> GetGrowingEPSCompanies([FromQuery] int years = 5)
        {
            try
            {
                var companies = await _service.GetConsecutiveGrowingEPSCompaniesAsync(years);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 