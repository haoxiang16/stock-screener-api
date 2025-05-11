using StockAPI.Interfaces;
using StockAPI.Models;
using StockAPI.Models.DTOs;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace StockAPI.Services
{
    public class StockAnalysisService : IStockAnalysisService
    {
        private readonly IStockRepository _stockRepository;

        public StockAnalysisService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<PaginatedResult<CompanyGrowthDTO>> GetConsecutiveGrowingEPSCompaniesAsync(int years = 5, int pageNumber = 1, int pageSize = 10)
        {
            // 取得所有年度 EPS 數據
            var statements = await _stockRepository.GetAllYearlyEPSAsync();

            // 以公司代碼分組並篩選出資料數量足夠的公司
            var qualifyingCompanyCodes = statements
                .GroupBy(s => s.CompanyCode)
                .Where(g => g.Count(s => s.Eps != null) >= years)
                .Where(g =>
                {
                    // 取得最近 years 年的資料，按年份升序排列
                    var sortedStatements = g
                        .Where(s => s.Eps != null)
                        .OrderByDescending(s => s.Year)
                        .Take(years)
                        .OrderBy(s => s.Year)
                        .ToList();
                    // 使用 Zip 檢查是否每年 EPS 均成長
                    return sortedStatements.Zip(sortedStatements.Skip(1), (prev, curr) => curr.Eps > prev.Eps)
                                           .All(isGrowing => isGrowing);
                })
                .Select(g => g.Key)
                .ToList();

            // 計算總數
            var totalCount = qualifyingCompanyCodes.Count;

            // 分頁處理
            var pagedCompanyCodes = qualifyingCompanyCodes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 一次性取得所有符合條件公司的詳細資料
            var companies = await _stockRepository.GetCompaniesByCodesAsync(pagedCompanyCodes);

            // 構造 DTO 資料，取得最新 years 年的 EPS 數據（按年份降序排列）
            var items = companies.Select(company =>
            {
                var sortedStatements = statements
                    .Where(s => s.CompanyCode == company.CompanyCode && s.Eps != null)
                    .OrderByDescending(s => s.Year)
                    .Take(years) 
                    .Select(s => new YearlyEpsDTO
                    {
                        Year = s.Year, 
                        Eps = s.Eps 
                    })
                    .ToList();

                return new CompanyGrowthDTO
                {
                    CompanyCode = company.CompanyCode,
                    CompanyName = company.CompanyName,
                    YearlyEps = sortedStatements
                };
            }).ToList();

            return new PaginatedResult<CompanyGrowthDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PaginatedResult<FinancialGrowthDTO>> GetConsecutiveGrowingFinancialsAsync(FinancialGrowthQueryDTO query)
        {
            // 取得所有年度財務數據
            var statements = await _stockRepository.GetYearlyFinancialsAsync();
            var maxYears = new[] { 
                query.EpsYears ?? 0, 
                query.OperatingMarginYears ?? 0, 
                query.GrossMarginYears ?? 0, 
                query.NetProfitMarginYears ?? 0 
            }.Max();

            // 以公司代碼分組並篩選出符合條件的公司
            var qualifyingCompanyCodes = statements
                .GroupBy(s => s.CompanyCode)
                .Where(g => g.Count() >= maxYears)
                .Where(g =>
                {
                    var sortedStatements = g
                        .OrderByDescending(s => s.Year)
                        .Take(maxYears)
                        .OrderBy(s => s.Year)
                        .ToList();

                    return IsMatchingCriteria(sortedStatements, query);
                })
                .Select(g => g.Key)
                .ToList();

            // 計算總數
            var totalCount = qualifyingCompanyCodes.Count;

            // 分頁處理
            var pagedCompanyCodes = qualifyingCompanyCodes
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            // 取得符合條件公司的詳細資料
            var companies = await _stockRepository.GetCompaniesByCodesAsync(pagedCompanyCodes);

            // 構造回傳資料
            var items = companies.Select(company =>
            {
                var financials = statements
                    .Where(s => s.CompanyCode == company.CompanyCode)
                    .OrderByDescending(s => s.Year)
                    .Take(maxYears)
                    .Select(s => new YearlyFinancialDTO
                    {
                        Year = s.Year,
                        Eps = s.Eps,
                        OperatingMargin = CalculateOperatingMargin(s),
                        GrossMargin = CalculateGrossMargin(s),
                        NetProfitMargin = CalculateNetProfitMargin(s)
                    })
                    // 如果設定了營業利益率門檻，則只返回達到門檻的年度數據
                    .Where(f => !query.MinOperatingMargin.HasValue || 
                               (f.OperatingMargin.HasValue && f.OperatingMargin >= query.MinOperatingMargin.Value))
                    .ToList();

                return new FinancialGrowthDTO
                {
                    CompanyCode = company.CompanyCode,
                    CompanyName = company.CompanyName,
                    YearlyFinancials = financials
                };
            })
            // 確保每個公司都有數據返回
            .Where(c => c.YearlyFinancials.Any())
            .ToList();

            return new PaginatedResult<FinancialGrowthDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        private bool IsMatchingCriteria(List<IncomeStatement> statements, FinancialGrowthQueryDTO query)
        {
            // 檢查是否有任何查詢條件
            if (!HasAnyQueryCriteria(query))
                return false;

            bool matchesCriteria = true;

            // 檢查營業利益率是否達到門檻（所有年度都必須達標）
            if (query.MinOperatingMargin.HasValue)
            {
                matchesCriteria &= statements.All(statement =>
                {
                    var margin = CalculateOperatingMargin(statement);
                    return margin.HasValue && margin >= query.MinOperatingMargin.Value;
                });

                if (!matchesCriteria) return false;  // 如果不符合門檻，直接返回 false
            }

            // EPS 成長檢查
            if (ShouldCheckEpsGrowth(query))
            {
                var epsStatements = statements.TakeLast(query.EpsYears.Value).ToList();
                if (epsStatements.Count < query.EpsYears.Value)
                    return false;
                matchesCriteria &=  IsEpsGrowing(epsStatements);
            }

            // 營業利益率成長檢查
            if (ShouldCheckOperatingMargin(query))
            {
                var operatingMarginStatements = statements.TakeLast(query.OperatingMarginYears.Value).ToList();
                if (operatingMarginStatements.Count < query.OperatingMarginYears.Value)
                    return false;
                matchesCriteria &= IsOperatingMarginGrowing(operatingMarginStatements);
            }

            // 毛利率成長檢查
            if (ShouldCheckGrossMargin(query))
            {
                var grossMarginStatements = statements.TakeLast(query.GrossMarginYears.Value).ToList();
                if (grossMarginStatements.Count < query.GrossMarginYears.Value)
                    return false;
                matchesCriteria &= IsGrossMarginGrowing(grossMarginStatements);
            }

            // 淨利率成長檢查
            if (ShouldCheckNetProfitMargin(query))
            {
                var netProfitMarginStatements = statements.TakeLast(query.NetProfitMarginYears.Value).ToList();
                if (netProfitMarginStatements.Count < query.NetProfitMarginYears.Value)
                    return false;
                matchesCriteria &= IsNetProfitMarginGrowing(netProfitMarginStatements);
            }

            return matchesCriteria;
        }

        private bool HasAnyQueryCriteria(FinancialGrowthQueryDTO query)
        {
            return ShouldCheckEpsGrowth(query) ||
                   ShouldCheckOperatingMargin(query) ||
                   ShouldCheckGrossMargin(query) ||
                   ShouldCheckNetProfitMargin(query);
        }

        private bool ShouldCheckEpsGrowth(FinancialGrowthQueryDTO query)
            => query.EpsYears.HasValue && query.EpsYears.Value > 0;

        private bool ShouldCheckOperatingMargin(FinancialGrowthQueryDTO query)
            => query.OperatingMarginYears.HasValue && query.OperatingMarginYears.Value > 0;

        private bool ShouldCheckGrossMargin(FinancialGrowthQueryDTO query)
            => query.GrossMarginYears.HasValue && query.GrossMarginYears.Value > 0;

        private bool ShouldCheckNetProfitMargin(FinancialGrowthQueryDTO query)
            => query.NetProfitMarginYears.HasValue && query.NetProfitMarginYears.Value > 0;

        private decimal? CalculateOperatingMargin(IncomeStatement statement)
        {
            if (statement.Revenue == 0 || statement.Revenue == null)
                return null;

            // 營業利益 = 營業收入 - 營業成本 - 營業費用
            var operatingIncome = statement.Revenue - statement.OperatingCosts - statement.OperatingExpenses;
            
            // 營業利益率 = (營業利益 ÷ 營業收入) × 100%
            var operatingMargin = (operatingIncome / statement.Revenue) * 100;
            return operatingMargin;
        }

        private decimal? CalculateGrossMargin(IncomeStatement statement)
        {
            if (statement.Revenue == 0 || statement.Revenue == null)
                return null;

            // 毛利 = 營業收入 - 營業成本
            var grossProfit = statement.Revenue - statement.OperatingCosts;
            
            // 毛利率 = 毛利 / 營業收入 × 100%
            var grossMargin = (grossProfit / statement.Revenue) * 100;
            return grossMargin;
        }

        private decimal? CalculateNetProfitMargin(IncomeStatement statement)
        {
            if (statement.Revenue == 0 || statement.Revenue == null)
                return null;

            // 稅後淨利率 = 稅後淨利 / 營業收入 × 100%
            var netProfitMargin = (statement.NetIncome / statement.Revenue) * 100;
            return netProfitMargin;
        }

        private bool IsEpsGrowing(List<IncomeStatement> statements)
        {
            var growthResults = statements.Zip(statements.Skip(1), (prev, curr) =>
            {
                var isValid = prev.Eps != null && curr.Eps != null;
                var isGrowing = isValid && curr.Eps > prev.Eps;
                return isGrowing;
            }).ToList();

            var isAllGrowing = growthResults.All(isGrowing => isGrowing);
            return isAllGrowing;
        }

        private bool IsOperatingMarginGrowing(List<IncomeStatement> statements)
        {
            var growthResults = statements.Zip(statements.Skip(1), (prev, curr) =>
            {
                var prevMargin = CalculateOperatingMargin(prev);
                var currMargin = CalculateOperatingMargin(curr);
                var isValid = prevMargin.HasValue && currMargin.HasValue;
                var isGrowing = isValid && currMargin > prevMargin;
                return isGrowing;
            }).ToList();

            var isAllGrowing = growthResults.All(isGrowing => isGrowing);
            return isAllGrowing;
        }

        private bool IsGrossMarginGrowing(List<IncomeStatement> statements)
        {
            var growthResults = statements.Zip(statements.Skip(1), (prev, curr) =>
            {
                var prevMargin = CalculateGrossMargin(prev);
                var currMargin = CalculateGrossMargin(curr);
                var isValid = prevMargin.HasValue && currMargin.HasValue;
                var isGrowing = isValid && currMargin > prevMargin;
                return isGrowing;
            }).ToList();

            var isAllGrowing = growthResults.All(isGrowing => isGrowing);
            return isAllGrowing;
        }

        private bool IsNetProfitMarginGrowing(List<IncomeStatement> statements)
        {
            var growthResults = statements.Zip(statements.Skip(1), (prev, curr) =>
            {
                var prevMargin = CalculateNetProfitMargin(prev);
                var currMargin = CalculateNetProfitMargin(curr);
                var isValid = prevMargin.HasValue && currMargin.HasValue;
                var isGrowing = isValid && currMargin > prevMargin;
                return isGrowing;
            }).ToList();

            var isAllGrowing = growthResults.All(isGrowing => isGrowing);
            return isAllGrowing;
        }
    }
}
