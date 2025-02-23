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

        public async Task<IEnumerable<CompanyGrowthDTO>> GetConsecutiveGrowingEPSCompaniesAsync(int years = 5)
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

            // 一次性取得所有符合條件公司的詳細資料
            var companies = await _stockRepository.GetCompaniesByCodesAsync(qualifyingCompanyCodes);

            // 構造 DTO 資料，取得最新 years 年的 EPS 數據（按年份升序排列）
            var result = companies.Select(company =>
            {
                var sortedStatements = statements
                    .Where(s => s.CompanyCode == company.CompanyCode && s.Eps != null)
                    .OrderBy(s => s.Year)
                    .TakeLast(years) 
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

            return result;
        }
    }
}
