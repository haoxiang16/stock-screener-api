using Microsoft.EntityFrameworkCore;
using StockAPI.Interfaces;
using StockAPI.Models;

namespace StockAPI.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly StockContext _context;

        public StockRepository(StockContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncomeStatement>> GetAllYearlyEPSAsync()
        {
            // 查詢第四季的年度 EPS 數據，以最新年度優先
            var yearlyEpsStatements = await _context.IncomeStatements
                .Where(s => s.Season == 4) // 只取第四季的數據（年度總 EPS）
                .OrderBy(s => s.CompanyCode)
                .ThenByDescending(s => s.Year) // 年份降序排列，最新的年份優先
                .ToListAsync();

            return yearlyEpsStatements;
        }

        public async Task<IEnumerable<Company>> GetCompaniesByCodesAsync(IEnumerable<string> companyCodes)
        {
            // 查詢指定公司代碼的公司資料
            var companies = await _context.Companies
                .Where(c => companyCodes.Contains(c.CompanyCode))
                .ToListAsync();

            return companies;
        }

        public async Task<IEnumerable<IncomeStatement>> GetYearlyFinancialsAsync()
        {
            // 查詢第四季的年度財務數據，以最新年度優先
            var yearlyFinancials = await _context.IncomeStatements
                .Where(s => s.Season == 4) // 只取第四季的數據
                .OrderBy(s => s.CompanyCode)
                .ThenByDescending(s => s.Year) // 年份降序排列，最新的年份優先
                .ToListAsync();

            return yearlyFinancials;
        }
    }
} 