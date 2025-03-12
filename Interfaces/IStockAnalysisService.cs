using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IStockAnalysisService
    {
        Task<IEnumerable<CompanyGrowthDTO>> GetConsecutiveGrowingEPSCompaniesAsync(int years = 5);
        Task<IEnumerable<FinancialGrowthDTO>> GetConsecutiveGrowingFinancialsAsync(FinancialGrowthQueryDTO query);
    }
} 