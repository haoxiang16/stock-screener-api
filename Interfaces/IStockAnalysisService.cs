using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IStockAnalysisService
    {
        Task<IEnumerable<FinancialGrowthDTO>> GetConsecutiveGrowingFinancialsAsync(FinancialGrowthQueryDTO query);
    }
} 