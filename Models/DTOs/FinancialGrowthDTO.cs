namespace StockAPI.Models.DTOs
{
    public class FinancialGrowthDTO
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public List<YearlyFinancialDTO> YearlyFinancials { get; set; } = new List<YearlyFinancialDTO>();
    }

    public class YearlyFinancialDTO
    {
        public int? Year { get; set; }
        public decimal? Eps { get; set; }
        public decimal? OperatingMargin { get; set; }  // 營業利益率
        public decimal? GrossMargin { get; set; }      // 毛利率
        public decimal? NetProfitMargin { get; set; }  // 稅後淨利率
    }
} 