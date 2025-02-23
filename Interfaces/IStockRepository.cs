using StockAPI.Models;

namespace StockAPI.Interfaces
{
    public interface IStockRepository
    {
        Task<IEnumerable<IncomeStatement>> GetAllYearlyEPSAsync();
        Task<IEnumerable<Company>> GetCompaniesByCodesAsync(IEnumerable<string> companyCodes);
    }
}
