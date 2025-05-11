using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IStockAnalysisService
    {
        Task<PaginatedResult<CompanyGrowthDTO>> GetConsecutiveGrowingEPSCompaniesAsync(int years = 5, int pageNumber = 1, int pageSize = 10);
        Task<PaginatedResult<FinancialGrowthDTO>> GetConsecutiveGrowingFinancialsAsync(FinancialGrowthQueryDTO query);
    }
} 