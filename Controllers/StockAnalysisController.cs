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

        [HttpGet("growing-financials")]
        public async Task<ActionResult<IEnumerable<FinancialGrowthDTO>>> GetGrowingFinancials(
            [FromQuery] int? epsYears,
            [FromQuery] int? operatingMarginYears,
            [FromQuery] int? grossMarginYears,
            [FromQuery] int? netProfitMarginYears,
            [FromQuery] decimal? minOperatingMargin,
            [FromQuery] int? minOperatingMarginYears)
        {
            try
            {
                var query = new FinancialGrowthQueryDTO
                {
                    EpsYears = epsYears,
                    OperatingMarginYears = operatingMarginYears,
                    GrossMarginYears = grossMarginYears,
                    NetProfitMarginYears = netProfitMarginYears,
                    MinOperatingMargin = minOperatingMargin,
                    MinOperatingMarginYears = minOperatingMarginYears
                };

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