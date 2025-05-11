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

        [HttpGet("growing-eps")]
        public async Task<ActionResult<PaginatedResult<CompanyGrowthDTO>>> GetGrowingEPSCompanies(
            [FromQuery] int years = 5,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetConsecutiveGrowingEPSCompaniesAsync(years, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("growing-financials")]
        public async Task<ActionResult<PaginatedResult<FinancialGrowthDTO>>> GetGrowingFinancials(
            [FromQuery] int? epsYears,
            [FromQuery] int? operatingMarginYears,
            [FromQuery] int? grossMarginYears,
            [FromQuery] int? netProfitMarginYears,
            [FromQuery] decimal? minOperatingMargin,
            [FromQuery] int? minOperatingMarginYears,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
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
                    MinOperatingMarginYears = minOperatingMarginYears,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _service.GetConsecutiveGrowingFinancialsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 