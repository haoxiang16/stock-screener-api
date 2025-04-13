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

        public async Task<string> GenerateFinancialReportAsync(string companyCode, int years = 3)
        {
            // 查詢公司資料
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyCode == companyCode);
            
            if (company == null)
            {
                return $"找不到代碼為 {companyCode} 的公司";
            }

            // 查詢公司最近幾年的財務報表，按年份降序排列
            var statements = await _context.IncomeStatements
                .Where(s => s.CompanyCode == companyCode && s.Season == 4)
                .OrderByDescending(s => s.Year) // 年份降序排列，確保取得最新的數據
                .Take(years)
                .ToListAsync();

            if (!statements.Any())
            {
                return $"公司 {company.CompanyName} ({companyCode}) 沒有可用的財務報表";
            }

            // 生成報告
            var report = new System.Text.StringBuilder();
            report.AppendLine($"============ {company.CompanyName} ({companyCode}) 財務報告 ============\n");
            
            // 基本財務數據
            report.AppendLine("【財務數據】(單位：百萬元)");
            report.AppendLine("年度\t\t營收\t\t毛利\t\t營業利益\t淨利\t\tEPS");
            report.AppendLine("--------------------------------------------------------");
            
            foreach (var statement in statements)
            {
                decimal revenue = statement.Revenue ?? 0;
                decimal operatingCosts = statement.OperatingCosts ?? 0;
                decimal operatingExpenses = statement.OperatingExpenses ?? 0;
                decimal grossProfit = revenue - operatingCosts;
                decimal operatingIncome = revenue - operatingCosts - operatingExpenses;
                decimal netIncome = statement.NetIncome ?? 0;
                
                report.AppendLine($"{statement.Year}\t\t{revenue/1000000:N0}\t\t{grossProfit/1000000:N0}\t\t{operatingIncome/1000000:N0}\t\t{netIncome/1000000:N0}\t\t{statement.Eps:N2}");
            }
            
            // 獲利能力
            report.AppendLine("\n【獲利能力】(單位：%)");
            report.AppendLine("年度\t\t毛利率\t\t營業利益率\t淨利率");
            report.AppendLine("--------------------------------------------------------");
            
            foreach (var statement in statements)
            {
                decimal revenue = statement.Revenue ?? 0;
                decimal operatingCosts = statement.OperatingCosts ?? 0;
                decimal operatingExpenses = statement.OperatingExpenses ?? 0;
                decimal netIncome = statement.NetIncome ?? 0;
                
                decimal grossMargin = revenue > 0 ? Math.Round((revenue - operatingCosts) * 100 / revenue, 2) : 0;
                decimal operatingMargin = revenue > 0 ? Math.Round((revenue - operatingCosts - operatingExpenses) * 100 / revenue, 2) : 0;
                decimal netProfitMargin = revenue > 0 ? Math.Round(netIncome * 100 / revenue, 2) : 0;
                
                report.AppendLine($"{statement.Year}\t\t{grossMargin:N2}%\t\t{operatingMargin:N2}%\t\t{netProfitMargin:N2}%");
            }
            
            // 年度成長率
            if (statements.Count > 1)
            {
                report.AppendLine("\n【年度成長率】(單位：%)");
                report.AppendLine("年度\t\tEPS成長\t\t營收成長\t淨利成長\t毛利率變動\t營益率變動\t淨利率變動");
                report.AppendLine("--------------------------------------------------------");
                
                bool continuousEpsGrowth = true;
                bool continuousRevenueGrowth = true;
                
                for (int i = 0; i < statements.Count - 1; i++)
                {
                    var current = statements[i]; // 當前年 (較新的年份)
                    var previous = statements[i + 1]; // 前一年 (較舊的年份)
                    
                    decimal currentEps = current.Eps ?? 0;
                    decimal previousEps = previous.Eps ?? 0;
                    decimal currentRevenue = current.Revenue ?? 0;
                    decimal previousRevenue = previous.Revenue ?? 0;
                    decimal currentNetIncome = current.NetIncome ?? 0;
                    decimal previousNetIncome = previous.NetIncome ?? 0;
                    
                    // 計算成長率
                    decimal epsGrowth = previousEps > 0 ? Math.Round((currentEps - previousEps) * 100 / previousEps, 2) : 0;
                    decimal revenueGrowth = previousRevenue > 0 ? Math.Round((currentRevenue - previousRevenue) * 100 / previousRevenue, 2) : 0;
                    decimal netIncomeGrowth = previousNetIncome > 0 ? Math.Round((currentNetIncome - previousNetIncome) * 100 / previousNetIncome, 2) : 0;
                    
                    // 計算比率
                    decimal currentGrossMargin = currentRevenue > 0 ? Math.Round(((currentRevenue - (current.OperatingCosts ?? 0)) * 100 / currentRevenue), 2) : 0;
                    decimal previousGrossMargin = previousRevenue > 0 ? Math.Round(((previousRevenue - (previous.OperatingCosts ?? 0)) * 100 / previousRevenue), 2) : 0;
                    decimal grossMarginChange = currentGrossMargin - previousGrossMargin;
                    
                    decimal currentOpMargin = currentRevenue > 0 ? Math.Round(((currentRevenue - (current.OperatingCosts ?? 0) - (current.OperatingExpenses ?? 0)) * 100 / currentRevenue), 2) : 0;
                    decimal previousOpMargin = previousRevenue > 0 ? Math.Round(((previousRevenue - (previous.OperatingCosts ?? 0) - (previous.OperatingExpenses ?? 0)) * 100 / previousRevenue), 2) : 0;
                    decimal opMarginChange = currentOpMargin - previousOpMargin;
                    
                    decimal currentNetMargin = currentRevenue > 0 ? Math.Round((currentNetIncome * 100 / currentRevenue), 2) : 0;
                    decimal previousNetMargin = previousRevenue > 0 ? Math.Round((previousNetIncome * 100 / previousRevenue), 2) : 0;
                    decimal netMarginChange = currentNetMargin - previousNetMargin;
                    
                    // 檢查連續成長
                    if (epsGrowth <= 0) continuousEpsGrowth = false;
                    if (revenueGrowth <= 0) continuousRevenueGrowth = false;
                    
                    // 輸出成長率
                    report.AppendLine($"{current.Year}\t\t{epsGrowth:N2}%\t\t{revenueGrowth:N2}%\t\t{netIncomeGrowth:N2}%\t\t{grossMarginChange:+0.00;-0.00}%\t\t{opMarginChange:+0.00;-0.00}%\t\t{netMarginChange:+0.00;-0.00}%");
                }
                
                // 成長評估
                report.AppendLine("\n【成長性評估】");
                report.AppendLine($"連續EPS成長：\t{(continuousEpsGrowth ? "是 ✓" : "否 ✗")}");
                report.AppendLine($"連續營收成長：\t{(continuousRevenueGrowth ? "是 ✓" : "否 ✗")}");
            }
            
            // 最新一年財務評級
            var latest = statements.FirstOrDefault(); // 第一個元素是最新的年份
            if (latest != null)
            {
                decimal revenue = latest.Revenue ?? 0;
                decimal operatingCosts = latest.OperatingCosts ?? 0;
                decimal operatingExpenses = latest.OperatingExpenses ?? 0;
                decimal netIncome = latest.NetIncome ?? 0;
                decimal eps = latest.Eps ?? 0;
                
                decimal grossMargin = revenue > 0 ? Math.Round((revenue - operatingCosts) * 100 / revenue, 2) : 0;
                decimal operatingMargin = revenue > 0 ? Math.Round((revenue - operatingCosts - operatingExpenses) * 100 / revenue, 2) : 0;
                decimal netProfitMargin = revenue > 0 ? Math.Round(netIncome * 100 / revenue, 2) : 0;
                
                report.AppendLine("\n【財務評級】");
                report.AppendLine($"EPS：\t\t{eps:N2} ({GetEpsRating(eps)})");
                report.AppendLine($"毛利率：\t{grossMargin:N2}% ({GetMarginRating(grossMargin)})");
                report.AppendLine($"營業利益率：\t{operatingMargin:N2}% ({GetMarginRating(operatingMargin)})");
                report.AppendLine($"淨利率：\t{netProfitMargin:N2}% ({GetMarginRating(netProfitMargin)})");
            }
            
            return report.ToString();
        }

        private string GetEpsRating(decimal eps)
        {
            if (eps <= 0) return "差";
            if (eps < 2) return "普通";
            if (eps < 5) return "良好";
            if (eps < 10) return "優秀";
            return "傑出";
        }

        private string GetMarginRating(decimal margin)
        {
            if (margin <= 0) return "差";
            if (margin < 10) return "普通";
            if (margin < 20) return "良好";
            if (margin < 30) return "優秀";
            return "傑出";
        }
    }
} 