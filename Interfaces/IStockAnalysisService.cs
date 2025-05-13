using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IStockAnalysisService
    {
        Task<PaginatedResult<FinancialGrowthDTO>> GetConsecutiveGrowingFinancialsAsync(FinancialGrowthQueryDTO query);
    }
} 