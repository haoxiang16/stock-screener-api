using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAPI.Interfaces;
using StockAPI.Models.DTOs;

namespace StockAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockAnalysisController : ControllerBase
    {
        private readonly IStockAnalysisService _service;

        public StockAnalysisController(IStockAnalysisService service)
        {
            _service = service;
        }
   
        [HttpPost("growing-financials")]
        public async Task<ActionResult<IEnumerable<FinancialGrowthDTO>>> GetGrowingFinancials(
            [FromBody] FinancialGrowthQueryDTO query)
        {
            try
            {
                var companies = await _service.GetConsecutiveGrowingFinancialsAsync(query);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 