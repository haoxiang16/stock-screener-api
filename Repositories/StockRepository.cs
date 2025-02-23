using Microsoft.EntityFrameworkCore;
using StockAPI.Interfaces;
using StockAPI.Models;

namespace StockAPI.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly stockContext _context;

        public StockRepository(stockContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncomeStatement>> GetAllYearlyEPSAsync()
        {
            return await _context.IncomeStatements
                .Where(s => s.Season == 4) // 只取第四季的數據（年度總 EPS）
                .OrderBy(s => s.CompanyCode)
                .ThenBy(s => s.Year)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetCompaniesByCodesAsync(IEnumerable<string> companyCodes)
        {
            return await _context.Companies
                .Where(c => companyCodes.Contains(c.CompanyCode))
                .ToListAsync();
        }
    }
} 