using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IStockAnalysisService
    {
        Task<IEnumerable<CompanyGrowthDTO>> GetConsecutiveGrowingEPSCompaniesAsync(int years = 5);
    }
} 